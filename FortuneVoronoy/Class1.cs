using System;
using System.Drawing;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Spectre.Console;

namespace FortuneVoronoy
{
    public class FortuneVoronoy
    {
        public Node beachLine;
        public SortedList<double, IEvent> events = new SortedList<double, IEvent>(); //Key = line
        private List<Line> completeLines = new List<Line>();
        public void Run(List<Seed> seeds)
        {
            FirstParabola(seeds[0]);
            seeds.RemoveAt(0);
            seeds.ForEach(x => events.Add(x.Point.Y, new NewSite(x.Point)));
            foreach (var eve in events)
            {
                if (eve.Value is NewSite)
                {
                    SiteEvent(eve.Value.AssociatedPoint);
                    seeds.RemoveAt(0);
                }
                else
                {
                    EdgeEvent((EdgeEvent)eve.Value);
                }
            }
            AnsiConsole.Render(RenderBeachLine(beachLine));
        }
        private void FirstParabola(Seed s)
        {
            beachLine = new Node(null)
            {
                Parabola = new Parabola()
                {
                    Focus = s.Point,
                },
                IsParabola = true,
                IsRoot = true
            };
            //beachLine.Parent = beachLine;
        }
        private void SiteEvent(PointD s)
        {
            Node toSplit = Search(s.X);
            Console.WriteLine($"Top node: {toSplit.IsRoot}");
            Console.WriteLine(toSplit.Parabola.Focus.X);
            Console.WriteLine(toSplit.Parabola.Focus.Y);
            PointD endPoint = new PointD(s.X, toSplit.Parabola.AtX(s.X, s.Y));
            double direction = toSplit.Parabola.DerivativeAtX(s.X, s.Y);
            Console.WriteLine($"x:{s.X}, Directrix:{s.Y}");
            Console.WriteLine(direction);
            Node replace = new Node(toSplit.Parent)
            {
                Ray = new Ray()
                {
                    EndPoint = endPoint,
                    Direction = new PointD(1, direction )
                }
            };
            replace.LeftChildren = new Node(replace)
            {
                Parabola = toSplit.Parabola,
                IsParabola = true,
            };
            replace.RightChildren = new Node(replace)
            {
                Ray = new Ray()
                {
                    EndPoint = endPoint,
                    Direction = new PointD(-1, -direction)
                }
            };
            replace.RightChildren.LeftChildren = new Node(replace.RightChildren)
            {
                Parabola = new Parabola
                {
                    Focus = new PointD(s.X, s.Y)
                },
                IsParabola = true
            };
            replace.RightChildren.RightChildren = new Node(replace.RightChildren)
            {
                Parabola = toSplit.Parabola,
                IsParabola = true
            };
            if (toSplit.IsRoot)
            {
                beachLine = replace;
            }
            else if (toSplit.IsLeftChildren)
            {
                toSplit.Parent.LeftChildren = replace;
            }
            else
            {
                toSplit.Parent.RightChildren = replace;
            }
            AnsiConsole.Render(RenderBeachLine(beachLine));
            CheckForEdgeEvent(replace.LeftChildren);
            CheckForEdgeEvent(replace.RightChildren.RightChildren);
            
        }
        private void CheckForEdgeEvent(Node n)
        {
            Node goingLeft = beachLine;
            while (goingLeft.LeftChildren != null)
            {
                goingLeft = goingLeft.LeftChildren;
            }
            if (goingLeft == n) //Parabola is leftmost element and therefore can't be squished.
            {
                Console.WriteLine("A");
                return;
            }
            Node goingRight = beachLine;
            while (goingRight.RightChildren != null)
            {
                goingRight = goingRight.RightChildren;
            }
            if (goingRight == n) //Parabola is rightmost element and therefore can't be squished.
            {
                Console.WriteLine("B");
                return;
            }
            Node leftRay = null;
            Node rightRay = null;
            if (n.Parent.Ray.EndPoint.X < n.Parabola.Focus.X) //Parent is to the left
            {
                leftRay = n.Parent;
                Node travel = leftRay;
                while (rightRay == null)
                {
                    if (travel.Parent.LeftChildren == travel) //If the traveling node is the left of the parent, then the parent is to the right and the ray is to the right of n (I think).
                    {
                        rightRay = travel.Parent;
                    }
                    else
                    {
                        travel = travel.Parent;
                    }
                }
            }
            else
            {
                AnsiConsole.Render(RenderBeachLine(beachLine));
                rightRay = n.Parent;
                Node travel = rightRay;
                while (leftRay == null)
                {
                    if (travel.Parent.RightChildren == travel) //If the traveling node is the right of the parent, then the parent is to the left and the ray is to the left of n (I think).
                    {
                        leftRay = travel.Parent;
                    }
                    else
                    {
                        travel = travel.Parent;
                    }
                }
            }
            PointD intersection = RaysIntersect(leftRay.Ray, rightRay.Ray);
            Console.WriteLine(intersection.X);
            if (intersection.X != 0)
            {
                PointD focus = n.Parabola.Focus;
                events.Add(intersection.Y + Math.Sqrt(Math.Pow(focus.X - intersection.X, 2) + Math.Pow(focus.Y - intersection.Y, 2)), new EdgeEvent(intersection, n));
            }

        }
        private PointD RaysIntersect(Ray r1, Ray r2)
        {
            //Explanation of method https://stackoverflow.com/a/2932601
            PointD a = r1.EndPoint;
            PointD b = r2.EndPoint;
            PointD v = r1.Direction; //Yes I know the letters are flipped.
            PointD u = r2.Direction;
            Console.WriteLine($"Point:{a.X},{a.Y}");
            Console.WriteLine($"Dir:{v.X},{v.Y}");
            Console.WriteLine($"Point:{b.X},{b.Y}");
            Console.WriteLine($"Dir:{u.X},{u.Y}");
            double det = -v.X * u.Y + v.Y * u.X;
            double dY = (b.Y - a.Y);
            double dX = (b.X - a.X);
            double t1 = dY * u.X - dX * u.Y;
            Console.WriteLine(det);
            if (Math.Sign(det) != Math.Sign(t1)) return new PointD();
            Console.WriteLine(Math.Sign(det) != Math.Sign(t1));
            double t2 = dY * v.X - dX * v.Y;
            if (Math.Sign(det) != Math.Sign(t2)) return new PointD();
            Console.WriteLine(Math.Sign(det) != Math.Sign(t2));
            return new PointD(a.X + v.X * (t1 / det), a.Y + v.Y * (t1 / det)); //Returns the intersection point.
        }
        private void EdgeEvent(EdgeEvent eve)
        {

        }
        private Node Search(Node node, Node next)
        {
            if (node.LeftChildren is not null)
            {
                Search(node.LeftChildren, node);
            }
            else if (node.IsParabola)
            {

            }
            else
            {
                Node n = node;
                while (!n.IsLeftChildren && !n.IsRoot)
                {
                    n = n.Parent;
                }
                Search(node.RightChildren, n.Parent);
            }
        }
        private (Node, Node) S(Node node, Node lagging, double directrix, double x)
        {
            if (!lagging.IsParabola)
            {
                if (node.LeftChildren is not null)
                {
                    return S(node.LeftChildren, node, directrix, x);
                }
                else if (node.RightChildren is not null)
                {
                    return S(node.RightChildren, node, directrix, x);
                }
                else
                {
                    return S(Next(node), node, directrix, x);
                }
            }
            else
            {
                //Intersection point => mx+n = ax^2+bx+c
                //m = direction.y / direction.x
                //n = initial point.y
                //0 = ax^2+(b-m)x+(c-n)
                //Quadratic formula where a = a, b = b-m, c = c-n
                Parabola p = lagging.Parabola;
                Ray r = node.Ray;
                double f = (p.Focus.Y - directrix);
                double a = 1 / (2 * f);
                double b = -p.Focus.X / f - (r.Direction.Y / r.Direction.X);
                double c = ((p.Focus.X * p.Focus.X) / (f) + (directrix + p.Focus.Y)) / 2 - r.EndPoint.Y;

                double discr = b * b - 4 * a * c;
                if (discr < 0)
                {/*Handle*/
                }
                if (discr == 0) //The intersection is at the focus, saves operation time.
                {
                    if (x < p.Focus.X)
                    {
                        return (lagging, null); //Point x hits lagging.
                    }
                    if (x == p.Focus.X)
                    {
                        //Handle point x being exactly at the intersection.
                    }
                    else
                    {
                        return S()
                    }
                }
            }
        }
        private Node Next(Node n)
        {
            while (!n.IsLeftChildren && !n.IsRoot)
            {
                n = n.Parent;
            }
            if (!n.IsRoot)
            {
                return n.Parent.RightChildren;
            }
            else
            {
                return null; //There are no more elements to the right.
            }
        }
        private Tree RenderBeachLine(Node n)
        {
            if (n is null) return new Tree("Done");
            Tree t = new Tree(n.IsParabola ? "P" : "R");
            t.AddNode(RenderBeachLine(n.LeftChildren));
            t.AddNode(RenderBeachLine(n.RightChildren));
            return t;
        }
    }
}

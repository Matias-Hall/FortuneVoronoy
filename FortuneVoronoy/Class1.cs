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
        private Dictionary<PointD, Polygon> polygons = new Dictionary<PointD, Polygon>();
        public List<Polygon> Run(List<Seed> seeds)
        {
            Console.WriteLine($"Before :{seeds.Count}");
            seeds = seeds.OrderBy(x => x.Point.Y).ToList();
            Console.WriteLine($"After :{seeds.Count}");
            Console.WriteLine($"First point: ({seeds[0].Point.X}, {seeds[0].Point.Y})");
            FirstParabola(seeds[0]);
            Console.WriteLine("First parabola added");
            AnsiConsole.Render(RenderBeachLine(beachLine));
            seeds.RemoveAt(0);
            Console.WriteLine($"After after:{seeds.Count}");
            seeds.ForEach(x => events.Add(x.Point.Y, new NewSite(x.Point)));
            events.Values.ToList().ForEach(x => Console.WriteLine($"{x.AssociatedPoint.X}, {x.AssociatedPoint.Y}"));
            Console.WriteLine($"After after after:{events.Count}");
            while (events.Any())
            {
                IEvent eve = events.Values[0];
                if (eve.GetType() == typeof(NewSite))
                {
                    Console.WriteLine("Going into new site event");
                    SiteEvent(eve.AssociatedPoint);
                }
                else
                {
                    Console.WriteLine("Going into edge event");
                    EdgeEvent((EdgeEvent)eve);
                }
                events.RemoveAt(0);
                Console.WriteLine($"After after after after:{events.Count}");
                events.Values.ToList().ForEach(x => Console.WriteLine($"{x.AssociatedPoint.X}, {x.AssociatedPoint.Y}"));
            }
            AnsiConsole.Render(RenderBeachLine(beachLine));
            return polygons.Values.ToList();
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
            polygons.Add(s.Point, new Polygon(s.Point));
            //beachLine.Parent = beachLine;
        }
        private void SiteEvent(PointD s)
        {
            Console.WriteLine($"Now adding: ({s.X}, {s.Y})");
            try
            {
                polygons.Add(s, new Polygon(s));
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Trouble point: ({s.X},{s.Y})");
                throw e;
            }
            Node toSplit = Search(s.X, s.Y);
            toSplit.Exists = false;
            Console.WriteLine($"Top node: {toSplit.IsRoot}");
            Console.WriteLine(toSplit.Parabola.Focus.X);
            Console.WriteLine(toSplit.Parabola.Focus.Y);
            PointD endPoint = new PointD(s.X, toSplit.Parabola.AtX(s.X, s.Y));
            double direction = toSplit.Parabola.DerivativeAtX(s.X, s.Y);
            Console.WriteLine($"x:{s.X}, Directrix:{s.Y}");
            Console.WriteLine($"Direction: {direction}");
            Node replace = new Node(toSplit.Parent)
            {
                Ray = new Ray()
                {
                    EndPoint = endPoint,
                    Direction = new PointD(-1, -direction)
                },
                IsRoot = toSplit.IsRoot //Since replace will be replacing toSplit, if toSplit was the root replace will be the new root.
            };
            replace.AssignLeftChildren(new Node(replace)
            {
                Parabola = toSplit.Parabola,
                IsParabola = true,
            });
            replace.AssignRightChildren(new Node(replace)
            {
                Ray = new Ray()
                {
                    EndPoint = endPoint,
                    Direction = new PointD(1, direction)
                }
            });
            replace.RightChildren.AssignLeftChildren(new Node(replace.RightChildren)
            {
                Parabola = new Parabola
                {
                    Focus = new PointD(s.X, s.Y)
                },
                IsParabola = true
            });
            replace.RightChildren.AssignRightChildren(new Node(replace.RightChildren)
            {
                Parabola = toSplit.Parabola,
                IsParabola = true
            });
            if (toSplit.IsRoot)
            {
                beachLine = replace;
                Console.WriteLine(beachLine.RightChildren.Parent == beachLine);

            }
            else if (toSplit.IsLeftChildren)
            {
                toSplit.Parent.AssignLeftChildren(replace);
                Console.WriteLine(toSplit.Parent.RightChildren.Parent == toSplit.Parent);

            }
            else
            {
                toSplit.Parent.AssignRightChildren(replace);
                Console.WriteLine(toSplit.Parent.RightChildren.Parent == toSplit.Parent);

            }

            AnsiConsole.Render(RenderBeachLine(beachLine));
            CheckForEdgeEvent(replace.LeftChildren);
            Console.WriteLine($"Still good: {replace.RightChildren.Parent == replace}");
            CheckForEdgeEvent(replace.RightChildren.RightChildren);
            Console.WriteLine($"Still good: {replace.RightChildren.Parent == replace}");

        }
        private void CheckForEdgeEvent(Node n)
        {
            Console.WriteLine("Checking for edge event");
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
            AnsiConsole.Render(RenderBeachLine(beachLine));
            Node leftRay = Prev(n);
            Console.WriteLine($"Left ray is null: {leftRay == null}");
            AnsiConsole.Render(RenderBeachLine(beachLine));
            Node rightRay = Next(n);
            Console.WriteLine($"Focus coords: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
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
        private void EdgeEvent(EdgeEvent eve) //TODO Just redo, doesn't work
        {
            AnsiConsole.Render(RenderBeachLine(beachLine));
            if (eve.SquishedParabola.Exists)
            {
                if (eve.SquishedParabola.IsLeftChildren)
                {
                    Node leftRay = Prev(eve.SquishedParabola);
                    Node leftPar = Prev(leftRay);
                    Node right = eve.SquishedParabola.Parent.RightChildren;
                    Node rightPar = right;
                    if (!right.IsParabola)
                    {
                        rightPar = Next(right);
                    }
                    double dir = rightPar.Parabola.DerivativeAtX(leftPar.Parabola.Focus.X, leftPar.Parabola.Focus.Y);
                    Node replacement = new Node(leftRay.Parent)
                    {
                        IsRoot = leftRay.Parent.IsRoot,
                        Ray = new Ray()
                        {
                            EndPoint = eve.AssociatedPoint,
                            Direction = new PointD(rightPar.Parabola.Focus.Y < leftPar.Parabola.Focus.Y ? 1 : -1, dir)
                        }
                    };
                    replacement.AssignLeftChildren(leftRay.LeftChildren);
                    if (leftRay.RightChildren == eve.SquishedParabola.Parent)
                    {
                        replacement.AssignRightChildren(right);
                    }
                    else
                    {
                        replacement.AssignRightChildren(leftRay.RightChildren);
                        replacement.RightChildren.AssignRightChildren(right);
                    }

                    if (leftRay.IsLeftChildren)
                    {
                        leftRay.Parent.AssignLeftChildren(replacement);
                    }
                    else if (!leftRay.IsRoot)
                    {
                        leftRay.Parent.AssignRightChildren(replacement);
                    }
                    else
                    {
                        beachLine = replacement;
                    }
                    Console.WriteLine("NEW beachline:");
                    AnsiConsole.Render(RenderBeachLine(beachLine));
                    polygons.GetValueOrDefault(leftPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    polygons.GetValueOrDefault(rightPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    polygons.GetValueOrDefault(eve.SquishedParabola.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    CheckForEdgeEvent(leftPar);
                    CheckForEdgeEvent(rightPar);
                }
                else
                {
                    Node rightRay = Next(eve.SquishedParabola);
                    Node rightPar = Next(rightRay);
                    Node left = eve.SquishedParabola.Parent.LeftChildren;
                    Node leftPar = left;
                    if (!left.IsParabola)
                    {
                        leftPar = Prev(eve.SquishedParabola.Parent);
                    }
                    double dir = rightPar.Parabola.DerivativeAtX(leftPar.Parabola.Focus.X, leftPar.Parabola.Focus.Y);
                    Node replacement = new Node(rightRay.Parent)
                    {
                        IsRoot = rightRay.IsRoot,
                        Ray = new Ray()
                        {
                            EndPoint = eve.AssociatedPoint,
                            Direction = new PointD(rightPar.Parabola.Focus.Y < leftPar.Parabola.Focus.Y ? 1 : -1, dir) //TODO When both foci y coords are equal it will be a special case, fix later. (Also same line above)
                        }
                    };
                    replacement.AssignRightChildren(rightRay.RightChildren);
                    if (rightRay == eve.SquishedParabola.Parent.Parent)
                    {
                        replacement.AssignLeftChildren(left);
                    }
                    else
                    {
                        replacement.AssignLeftChildren(rightRay.LeftChildren);
                        eve.SquishedParabola.Parent.Parent.AssignRightChildren(left);
                        //replacement.LeftChildren.AssignRightChildren(left);
                    }
                    if (rightRay.IsLeftChildren)
                    {
                        rightRay.Parent.AssignLeftChildren(replacement);
                    }
                    else if (!rightRay.IsRoot)
                    {
                        rightRay.Parent.AssignRightChildren(replacement);
                    }
                    else
                    {
                        beachLine = replacement;
                    }
                    Console.WriteLine("NEW beachline:");
                    AnsiConsole.Render(RenderBeachLine(beachLine));
                    polygons.GetValueOrDefault(leftPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    polygons.GetValueOrDefault(rightPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    polygons.GetValueOrDefault(eve.SquishedParabola.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                    CheckForEdgeEvent(leftPar);
                    CheckForEdgeEvent(rightPar);
                }
            }
        }
        private Node Search(double x, double directrix)
        {
            if (beachLine.RightChildren == null && beachLine.LeftChildren == null) return beachLine; //First case where no rays exist.
            Console.WriteLine("E");
            Console.WriteLine($"X to search {x}");
            Node lagging = beachLine;
            while (lagging.LeftChildren != null) //Initializes lagging to leftmost element and node to the one after.
            {
                lagging = lagging.LeftChildren;
            }
            Node node = lagging.Parent;
            int index = 0;
            while (node != null)
            {
                Console.WriteLine(index);
                index++;
                if (!lagging.IsParabola)
                {
                    lagging = node;
                    node = Next(node, false, false, true);
                }
                else
                {
                    //Intersection point => mx+n = ax^2+bx+c
                    //m = direction.y / direction.x
                    //n = m*(-initial x) + initial y
                    //0 = ax^2+(b-m)x+(c-n)
                    //Quadratic formula where a = a, b = b-m, c = c-n
                    Parabola p = lagging.Parabola;
                    Console.WriteLine($"Focus:({p.Focus.X}, {p.Focus.Y})");
                    Console.WriteLine(directrix);
                    Ray r = node.Ray;
                    Console.WriteLine($"Ray endpoint:({r.EndPoint.X}, {r.EndPoint.Y})");
                    Console.WriteLine($"Ray direction: {r.Direction.X}");
                    Console.WriteLine($"Ray is root: {node.IsRoot}");
                    double f = (p.Focus.Y - directrix);
                    double a = 1 / (2 * f);
                    double m = (r.Direction.Y / r.Direction.X);
                    double b = -p.Focus.X / f - m;
                    double c = ((p.Focus.X * p.Focus.X) / (f) + (directrix + p.Focus.Y)) / 2 - (m * -r.EndPoint.X + r.EndPoint.Y);

                    double discr = b * b - 4 * a * c;
                    if (discr < 0)
                    {
                        Console.WriteLine($"Discr less than 0: {discr}");
                        lagging = node;
                        node = Next(node, false, false, true);
                    }
                    else if (discr == 0) //The intersection is at the focus, saves operation time.
                    {
                        if (x < p.Focus.X)
                        {
                            return lagging; //Point x hits lagging.
                        }
                        else if (x == p.Focus.X)
                        {
                            return lagging;
                            //TODO: What happens when the x coordinate being searched for is exactly on the intersection. What to return?
                        }
                        else
                        {
                            Console.WriteLine($"One solution discr 0");
                            lagging = node;
                            node = Next(node, false, false, true);
                        }
                    }
                    else
                    {
                        double intersection = 0.0d;
                        if (r.Direction.X > 0) //Chooses which zero to get based on which ray of the line that intersects the parabola it has.
                        {
                            intersection = (-b - Math.Sqrt(discr)) / (2 * a);
                        }
                        else
                        {
                            intersection = (-b + Math.Sqrt(discr)) / (2 * a);
                        }
                        if (x < intersection)
                        {
                            Console.WriteLine($"Yes dice: intersection = {intersection}");
                            return lagging; //Point x hits lagging.
                        }
                        else
                        {
                            Console.WriteLine($"No dice: intersection = {intersection}");
                            lagging = node;
                            Console.WriteLine($"Is root: {node.IsRoot}");
                            Console.WriteLine($"Is ray: {!node.IsParabola}");
                            node = Next(node, false, false, true);
                        }
                    }
                }
            }
            //node being null means lagging is at the last element, since no other element capture x, the last element must.
            return lagging;
        }
        private Node Next(Node n, bool cameFromLeft = false, bool cameFromRight = false, bool firstCall = true) //Should be correct
        {
            Console.WriteLine($"Is root: {n.IsRoot}");
            if (n.LeftChildren == null)
            {
                Console.WriteLine($"Point: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
                Console.WriteLine("No left children");
                if (!firstCall) return n;
                else
                {
                    if (n.IsLeftChildren) return Next(n.Parent, true, false, false);
                    else if (firstCall && n.Parent.RightChildren == n)
                    {
                        Console.WriteLine("Activated A");
                        return Next(n.Parent, false, true, false);
                    }
                    else return n;
                }
            }
            else
            {
                AnsiConsole.Render(RenderBeachLine(n));
                if (!firstCall)
                {
                    if (cameFromLeft)
                    {
                        Console.WriteLine("Activated C");
                        return n;
                    }
                    else if (cameFromRight)
                    {
                        Console.WriteLine($"Parent is null: {n.Parent == null}");
                        Console.WriteLine($"Is leftchild: {n.Parent?.LeftChildren == n}");
                        Console.WriteLine($"Is rightchild: {n.Parent?.RightChildren == n}");
                        if (n.IsLeftChildren)
                        {
                            Console.WriteLine("Activated B");
                            return Next(n.Parent, true, false, false);
                        }
                        else if (n.IsRoot) return null;
                        else return Next(n.Parent, false, true, false);
                    }
                    else return Next(n.LeftChildren, false, false, false);
                }
                else
                {
                    Console.WriteLine("YE");
                    return Next(n.RightChildren, false, false, false);
                }
            }
        }
        private Node Prev(Node n, bool cameFromLeft = false, bool cameFromRight = false, bool firstCall = true)
        {
            if (n.LeftChildren == null)
            {
                if (!firstCall) return n;
                else if (n.IsLeftChildren)
                {
                    return Prev(n.Parent, true, false, false);
                }
                else
                {
                    return Prev(n.Parent, false, true, false);
                }
            }
            else
            {
                if (firstCall) return Prev(n.LeftChildren, false, false, false);
                else if (cameFromRight) return n;
                else if (cameFromLeft)
                {
                    if (n.IsRoot) return null;
                    else if (n.IsLeftChildren) return Prev(n.Parent, true, false, false);
                    else return Prev(n.Parent, false, true, false);
                }
                else
                {
                    return Prev(n.RightChildren, false, false, false);
                }
            }


        }
        private Tree RenderBeachLine(Node n)
        {
            if (n is null) return new Tree("Done");
            try
            {
                if (n == n.LeftChildren || n == n.RightChildren || n.LeftChildren?.LeftChildren == n || n.LeftChildren?.RightChildren == n || n.RightChildren?.LeftChildren == n || n.RightChildren?.RightChildren == n)
                {
                    Console.WriteLine(n.IsLeftChildren);
                    Console.WriteLine(n.RightChildren.IsLeftChildren);
                    Console.WriteLine(n.Parent.Parent.Parent.IsRoot);
                    if (n.IsParabola)
                    {
                        Console.WriteLine($"Loop caught at: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
                    }
                    throw new Exception($"Looped: in left side: {n == n.LeftChildren}, is parabola: {n.IsParabola}, looped in parent side: {n.Parent == n}");
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            Tree t = new Tree(n.IsParabola ? $"P ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})" : "R");
            t.AddNode(RenderBeachLine(n.LeftChildren));
            t.AddNode(RenderBeachLine(n.RightChildren));
            return t;
        }
    }
}

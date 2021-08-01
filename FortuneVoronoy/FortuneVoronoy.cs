using System;
using System.Drawing;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Spectre.Console;
using System.Diagnostics;

namespace FortuneVoronoy
{
    public class FortuneVoronoy
    {
        public Node beachLine;
        public SortedList<PointD, IEvent> events = new SortedList<PointD, IEvent>(new EventComparer()); //Key = line
        private List<Line> completeLines = new List<Line>();
        private Dictionary<PointD, Polygon> polygons = new Dictionary<PointD, Polygon>();
        private Dictionary<PointD, NewPolygon> newPolygons = new Dictionary<PointD, NewPolygon>();
        public List<NewPolygon> ShellRun(List<Seed> seeds) //Used to test the NewPolygon which uses lines instead of vertices.
        {
            Run(seeds);
            return newPolygons.Values.ToList();
        }
        public List<Polygon> Run(List<Seed> seeds)
        {
            Debug.WriteLine($"Before :{seeds.Count}");
            seeds = seeds.OrderBy(x => x.Point.Y).ToList();
            Debug.WriteLine($"After :{seeds.Count}");
            Debug.WriteLine($"First point: ({seeds[0].Point.X}, {seeds[0].Point.Y})");
            double firstDirectrixLoc = seeds[0].Point.Y;
            double secondEvent = seeds.Where(x => x.Point.Y > firstDirectrixLoc).First().Point.Y;
            while (seeds[0].Point.Y == firstDirectrixLoc)
            {
                FirstParabola(seeds[0], secondEvent);
                Debug.WriteLine("First parabola added");
#if DEBUG
                AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
                seeds.RemoveAt(0);
            }            
            Debug.WriteLine($"After after:{seeds.Count}");
            foreach (var seed in seeds)
            {
                try
                {
                    events.Add(seed.Point, new NewSite(seed.Point));
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"({seed.Point.X}, {seed.Point.Y})");
                    Console.Write($"{seeds.Where(x => x.Point == seed.Point).Count()}");
                    foreach (var x in seeds)
                    {
                        Console.Write($"({x.Point.X}, {x.Point.Y})");
                    }
                    throw e;
                }
            }
            events.Values.ToList().ForEach(x => Debug.WriteLine($"{x.AssociatedPoint.X}, {x.AssociatedPoint.Y}"));
            Debug.WriteLine($"After after after:{events.Count}");
            while (events.Any())
            {
                IEvent eve = events.Values[0];
                if (eve.GetType() == typeof(NewSite))
                {
                    Debug.WriteLine("Going into new site event");
                    SiteEvent(eve.AssociatedPoint);
                }
                else
                {
                    Debug.WriteLine("Going into edge event");
                    EdgeEvent((EdgeEvent)eve);
                }
                events.RemoveAt(0);
                Debug.WriteLine($"After after after after:{events.Count}");
                events.Values.ToList().ForEach(x => Debug.WriteLine($"{x.AssociatedPoint.X}, {x.AssociatedPoint.Y}"));
            }
#if DEBUG
            AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
            return polygons.Values.ToList();
        }
        private void FirstParabola(Seed s, double nextEvent)
        {
            if (beachLine == null)
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
            }
            else
            {
                Node rightMost = beachLine;
                while (rightMost.RightChildren != null)
                {
                    rightMost = rightMost.RightChildren;
                }
                double xCoord = (rightMost.Parabola.Focus.X + s.Point.X) / 2;
                Node replace = new Node(rightMost.Parent)
                {
                    IsRoot = rightMost.IsRoot,
                    Ray = new Ray()
                    {
                        EndPoint = new PointD(xCoord, rightMost.Parabola.AtX(xCoord, nextEvent)),
                        Direction = new PointD(0, 1) //Vertical line
                    }
                };
                Node newPar = new Node(null)
                {
                    IsParabola = true,
                    Parabola = new Parabola()
                    {
                        Focus = s.Point
                    }
                };
                if (rightMost == beachLine)
                {
                    replace.AssignLeftChildren(rightMost);
                    replace.AssignRightChildren(newPar);
                    beachLine = replace;
                    rightMost.IsRoot = false;
                }
                else
                {
                    rightMost.Parent.AssignRightChildren(replace);
                    replace.AssignLeftChildren(rightMost);
                    replace.AssignRightChildren(newPar);
                }
            }
            newPolygons.Add(s.Point, new NewPolygon(s.Point));
            polygons.Add(s.Point, new Polygon(s.Point));
        }
        private void SiteEvent(PointD s)
        {
            Debug.WriteLine($"Now adding: ({s.X}, {s.Y})");
            try
            {
                polygons.Add(s, new Polygon(s));
                newPolygons.Add(s, new NewPolygon(s));
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine($"Trouble point: ({s.X},{s.Y})");
                throw e;
            }
            Node toSplit = Search(s.X, s.Y);
            toSplit.Exists = false;
            Debug.WriteLine($"Top node: {toSplit.IsRoot}");
            Debug.WriteLine(toSplit.Parabola.Focus.X);
            Debug.WriteLine(toSplit.Parabola.Focus.Y);
            PointD endPoint = new PointD(s.X, toSplit.Parabola.AtX(s.X, s.Y));
            double direction = toSplit.Parabola.DerivativeAtX(s.X, s.Y);
            Debug.WriteLine($"x:{s.X}, Directrix:{s.Y}");
            Debug.WriteLine($"Direction: {direction}");
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
                Debug.WriteLine(beachLine.RightChildren.Parent == beachLine);

            }
            else if (toSplit.IsLeftChildren)
            {
                toSplit.Parent.AssignLeftChildren(replace);
                Debug.WriteLine(toSplit.Parent.RightChildren.Parent == toSplit.Parent);

            }
            else
            {
                toSplit.Parent.AssignRightChildren(replace);
                Debug.WriteLine(toSplit.Parent.RightChildren.Parent == toSplit.Parent);

            }
#if DEBUG
            AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
            CheckForEdgeEvent(replace.LeftChildren);
            Debug.WriteLine($"Still good: {replace.RightChildren.Parent == replace}");
            CheckForEdgeEvent(replace.RightChildren.RightChildren);
            Debug.WriteLine($"Still good: {replace.RightChildren.Parent == replace}");

        }
        private void CheckForEdgeEvent(Node n)
        {
            Debug.WriteLine("Checking for edge event");
            Node goingLeft = beachLine;
            while (goingLeft.LeftChildren != null)
            {
                goingLeft = goingLeft.LeftChildren;
            }
            if (goingLeft == n) //Parabola is leftmost element and therefore can't be squished.
            {
                Debug.WriteLine("A");
                return;
            }
            Node goingRight = beachLine;
            while (goingRight.RightChildren != null)
            {
                goingRight = goingRight.RightChildren;
            }
            if (goingRight == n) //Parabola is rightmost element and therefore can't be squished.
            {
                Debug.WriteLine("B");
                return;
            }
            Node leftRay = Prev(n);
            Node rightRay = Next(n);
#if DEBUG
            AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
            Debug.WriteLine($"Left ray is null: {leftRay == null}");
            Debug.WriteLine($"Focus coords: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
            PointD intersection = RaysIntersect(leftRay.Ray, rightRay.Ray);
            Debug.WriteLine(intersection.X);
            if (intersection.X != 0)
            {
                PointD focus = n.Parabola.Focus;
                PointD directrixLoc = new PointD(intersection.X, intersection.Y + Math.Sqrt(Math.Pow(focus.X - intersection.X, 2) + Math.Pow(focus.Y - intersection.Y, 2)));
                if (events.ContainsKey(directrixLoc))
                {
                    IEvent eve = events.GetValueOrDefault(directrixLoc);
                    if (eve is EdgeEvent)
                    {
                        Debug.WriteLine($"Updating intersection: ({intersection.X}, {intersection.Y})");
                        EdgeEvent edgeEve = (EdgeEvent)eve;
                        edgeEve.SquishedParabolas.Add(n);
                    }
                    else
                    {
                        throw new ArgumentException("Huh?");
                    }
                }
                else
                {
                    Debug.WriteLine($"Adding intersection: ({intersection.X}, {intersection.Y})");
                    events.Add(directrixLoc, new EdgeEvent(intersection, n));
                }
            }

        }
        private PointD RaysIntersect(Ray r1, Ray r2)
        {
            //Explanation of method https://stackoverflow.com/a/2932601
            PointD a = r1.EndPoint;
            PointD b = r2.EndPoint;
            PointD v = r1.Direction; //Yes I know the letters are flipped.
            PointD u = r2.Direction;
            Debug.WriteLine($"Point:{a.X},{a.Y}");
            Debug.WriteLine($"Dir:{v.X},{v.Y}");
            Debug.WriteLine($"Point:{b.X},{b.Y}");
            Debug.WriteLine($"Dir:{u.X},{u.Y}");
            double det = -v.X * u.Y + v.Y * u.X;
            double dY = (b.Y - a.Y);
            double dX = (b.X - a.X);
            double t1 = dY * u.X - dX * u.Y;
            Debug.WriteLine(det);
            if (Math.Sign(det) != Math.Sign(t1)) return new PointD();
            Debug.WriteLine(Math.Sign(det) != Math.Sign(t1));
            double t2 = dY * v.X - dX * v.Y;
            if (Math.Sign(det) != Math.Sign(t2)) return new PointD();
            Debug.WriteLine(Math.Sign(det) != Math.Sign(t2));
            return new PointD(a.X + v.X * (t1 / det), a.Y + v.Y * (t1 / det)); //Returns the intersection point.
        }
        private void EdgeEvent(EdgeEvent eve)
        {
#if DEBUG
            AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
            foreach (var par in eve.SquishedParabolas)
            {
                if (par.Exists)
                {
                    Debug.WriteLine($"Vertex being added: ({eve.AssociatedPoint.X}, {eve.AssociatedPoint.Y})");
                    if (par.IsLeftChildren)
                    {
                        Node leftRay = Prev(par);
                        Node leftPar = Prev(leftRay);
                        Node right = par.Parent.RightChildren;
                        Node rightPar = Next(Next(par));
                        Debug.WriteLine($"Left parabola is: ({leftPar.Parabola.Focus.X}, {leftPar.Parabola.Focus.Y})");
                        Debug.WriteLine($"Right parabola is: ({rightPar.Parabola.Focus.X}, {rightPar.Parabola.Focus.Y})");
                        double dir = Math.Abs(rightPar.Parabola.DerivativeAtX(leftPar.Parabola.Focus.X, leftPar.Parabola.Focus.Y));
                        if (rightPar.Parabola.Focus.X < leftPar.Parabola.Focus.X)
                        {
                            dir = -dir; //If the condition is true, the ray travels down (negative y dir) rather than up.
                        }
                        Debug.WriteLine($"Computed direction: {dir}");
                        PointD dirPt;
                        if (rightPar.Parabola.Focus.Y < leftPar.Parabola.Focus.Y)
                        {
                            dirPt = new PointD(1, Math.Abs(dir));
                        }
                        else if (rightPar.Parabola.Focus.Y > leftPar.Parabola.Focus.Y)
                        {
                            dirPt = new PointD(-1, Math.Abs(dir));
                        }
                        else
                        {
                            dirPt = new PointD(0, 1); //If both parabolas lie on a horizontal line, the ray from their intersection will be vertical.
                        }
                        Node replacement = new Node(leftRay.Parent)
                        {
                            IsRoot = leftRay.IsRoot,
                            Ray = new Ray()
                            {
                                EndPoint = eve.AssociatedPoint,
                                Direction = dirPt
                            }
                        };
                        replacement.AssignLeftChildren(leftRay.LeftChildren);
                        if (leftRay.RightChildren == par.Parent)
                        {
                            replacement.AssignRightChildren(right);
                        }
                        else
                        {
                            replacement.AssignRightChildren(leftRay.RightChildren);
                            par.Parent.Parent.AssignLeftChildren(right);
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
                        Debug.WriteLine("NEW beachline:");
#if DEBUG
                        AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
                        //TODO Make polygons work correctly
                        #region New polygons
                        newPolygons.GetValueOrDefault(leftPar.Parabola.Focus).Edges.Add(new Line(leftRay.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(rightPar.Parabola.Focus).Edges.Add(new Line(par.Parent.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(par.Parabola.Focus).Edges.Add(new Line(leftRay.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(par.Parabola.Focus).Edges.Add(new Line(par.Parent.Ray.EndPoint, eve.AssociatedPoint));
                        #endregion
                        polygons.GetValueOrDefault(leftPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        polygons.GetValueOrDefault(rightPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        polygons.GetValueOrDefault(par.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        CheckForEdgeEvent(leftPar);
                        CheckForEdgeEvent(rightPar);
                    }
                    else
                    {
                        Node rightRay = Next(par);
                        Node rightPar = Next(rightRay);
                        Node left = par.Parent.LeftChildren;
                        Node leftPar = Prev(Prev(par));
                        double dir = Math.Abs(rightPar.Parabola.DerivativeAtX(leftPar.Parabola.Focus.X, leftPar.Parabola.Focus.Y));
                        if (rightPar.Parabola.Focus.X < leftPar.Parabola.Focus.X)
                        {
                            dir = -dir; //If the condition is true, the ray travels down (negative y dir) rather than up.
                        }
                        PointD dirPt;
                        if (rightPar.Parabola.Focus.Y < leftPar.Parabola.Focus.Y)
                        {
                            dirPt = new PointD(1, dir);
                        }
                        else if (rightPar.Parabola.Focus.Y > leftPar.Parabola.Focus.Y)
                        {
                            dirPt = new PointD(-1, dir);
                        }
                        else
                        {
                            dirPt = new PointD(0, 1); //If both parabolas lie on a horizontal line, the ray from their intersection will be vertical.
                        }
                        Node replacement = new Node(rightRay.Parent)
                        {
                            IsRoot = rightRay.IsRoot,
                            Ray = new Ray()
                            {
                                EndPoint = eve.AssociatedPoint,
                                Direction = dirPt
                            }
                        };
                        replacement.AssignRightChildren(rightRay.RightChildren);
                        if (rightRay == par.Parent.Parent)
                        {
                            replacement.AssignLeftChildren(left);
                        }
                        else
                        {
                            replacement.AssignLeftChildren(rightRay.LeftChildren);
                            par.Parent.Parent.AssignRightChildren(left);
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
                        Debug.WriteLine("NEW beachline:");
#if DEBUG
                        AnsiConsole.Render(RenderBeachLine(beachLine));
#endif
                        #region New polygons
                        newPolygons.GetValueOrDefault(leftPar.Parabola.Focus).Edges.Add(new Line(par.Parent.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(rightPar.Parabola.Focus).Edges.Add(new Line(rightRay.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(par.Parabola.Focus).Edges.Add(new Line(par.Parent.Ray.EndPoint, eve.AssociatedPoint));
                        newPolygons.GetValueOrDefault(par.Parabola.Focus).Edges.Add(new Line(rightRay.Ray.EndPoint, eve.AssociatedPoint));
                        #endregion
                        polygons.GetValueOrDefault(leftPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        polygons.GetValueOrDefault(rightPar.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        polygons.GetValueOrDefault(par.Parabola.Focus).Vertices.Add(eve.AssociatedPoint);
                        CheckForEdgeEvent(leftPar);
                        CheckForEdgeEvent(rightPar);
                    }
                    par.Exists = false; //Avoids multiple intersection events based on the same parabola segment.
                }
            }
        }
        private Node Search(double x, double directrix)
        {
            if (beachLine.RightChildren == null && beachLine.LeftChildren == null) return beachLine; //First case where no rays exist.
            Debug.WriteLine("E");
            Debug.WriteLine($"X to search {x}");
            Node lagging = beachLine;
            while (lagging.LeftChildren != null) //Initializes lagging to leftmost element and node to the one after.
            {
                lagging = lagging.LeftChildren;
            }
            Node node = lagging.Parent;
            int index = 0;
            while (node != null)
            {
                Debug.WriteLine(index);
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
                    Debug.WriteLine($"Focus:({p.Focus.X}, {p.Focus.Y})");
                    Debug.WriteLine(directrix);
                    Ray r = node.Ray;
                    Debug.WriteLine($"Ray endpoint:({r.EndPoint.X}, {r.EndPoint.Y})");
                    Debug.WriteLine($"Ray direction: {r.Direction.X}");
                    Debug.WriteLine($"Ray is root: {node.IsRoot}");
                    double intersection = 0.0d;
                    if (r.Direction.X == 0) //If ray is vertical
                    {
                        intersection = r.EndPoint.X;
                    }
                    else
                    {
                        double m = (r.Direction.Y / r.Direction.X);
                        double f = (p.Focus.Y - directrix);
                        double a = 1 / (2 * f);
                        double b = -p.Focus.X / f - m;
                        double c = ((p.Focus.X * p.Focus.X) / (f) + (directrix + p.Focus.Y)) / 2 - (m * -r.EndPoint.X + r.EndPoint.Y);

                        double discr = b * b - 4 * a * c;
                        if (discr < 0)
                        {
                            Debug.WriteLine($"Discr less than 0: {discr}");
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
                                Debug.WriteLine($"One solution discr 0");
                                lagging = node;
                                node = Next(node, false, false, true);
                            }
                        }
                        else
                        {
                            if (r.Direction.X > 0) //Chooses which zero to get based on which ray of the line that intersects the parabola it has.
                            {
                                intersection = (-b - Math.Sqrt(discr)) / (2 * a);
                            }
                            else
                            {
                                intersection = (-b + Math.Sqrt(discr)) / (2 * a);
                            }
                        }
                    }
                    if (x < intersection)
                    {
                        Debug.WriteLine($"Yes dice: intersection = {intersection}");
                        return lagging; //Point x hits lagging.
                    }
                    else
                    {
                        Debug.WriteLine($"No dice: intersection = {intersection}");
                        lagging = node;
                        Debug.WriteLine($"Is root: {node.IsRoot}");
                        Debug.WriteLine($"Is ray: {!node.IsParabola}");
                        node = Next(node, false, false, true);
                    }
                }
            }
            //node being null means lagging is at the last element, since no other element capture x, the last element must.
            return lagging;
        }
        private Node Next(Node n, bool cameFromLeft = false, bool cameFromRight = false, bool firstCall = true) //Should be correct
        {
            Debug.WriteLine($"Is root: {n.IsRoot}");
            if (n.LeftChildren == null)
            {
                Debug.WriteLine($"Point: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
                Debug.WriteLine("No left children");
                if (!firstCall) return n;
                else
                {
                    if (n.IsLeftChildren) return Next(n.Parent, true, false, false);
                    else if (firstCall && n.Parent.RightChildren == n)
                    {
                        Debug.WriteLine("Activated A");
                        return Next(n.Parent, false, true, false);
                    }
                    else return n;
                }
            }
            else
            {
#if DEBUG
                AnsiConsole.Render(RenderBeachLine(n));
#endif
                if (!firstCall)
                {
                    if (cameFromLeft)
                    {
                        Debug.WriteLine("Activated C");
                        return n;
                    }
                    else if (cameFromRight)
                    {
                        Debug.WriteLine($"Parent is null: {n.Parent == null}");
                        Debug.WriteLine($"Is leftchild: {n.Parent?.LeftChildren == n}");
                        Debug.WriteLine($"Is rightchild: {n.Parent?.RightChildren == n}");
                        if (n.IsLeftChildren)
                        {
                            Debug.WriteLine("Activated B");
                            return Next(n.Parent, true, false, false);
                        }
                        else if (n.IsRoot) return null;
                        else return Next(n.Parent, false, true, false);
                    }
                    else return Next(n.LeftChildren, false, false, false);
                }
                else
                {
                    Debug.WriteLine("YE");
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
                    Debug.WriteLine(n.IsLeftChildren);
                    Debug.WriteLine(n.RightChildren.IsLeftChildren);
                    Debug.WriteLine(n.Parent.Parent.Parent.IsRoot);
                    if (n.IsParabola)
                    {
                        Debug.WriteLine($"Loop caught at: ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y})");
                    }
                    throw new Exception($"Looped: in left side: {n == n.LeftChildren}, is parabola: {n.IsParabola}, looped in parent side: {n.Parent == n}");
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e);
            }
            Tree t = new Tree(n.IsParabola ? $"P ({n.Parabola.Focus.X}, {n.Parabola.Focus.Y}) {n.IsRoot}" : $"R {n.IsRoot}");
            t.AddNode(RenderBeachLine(n.LeftChildren));
            t.AddNode(RenderBeachLine(n.RightChildren));
            return t;
        }
    }
    public class EventComparer : IComparer<PointD>
    {
        public int Compare(PointD a, PointD b) //Orders in y direction first, from smallest to largest, then in 
        {
            int yComp = a.Y.CompareTo(b.Y);
            if (yComp == 0)
            {
                return a.X.CompareTo(b.X);
            }
            else
            {
                return yComp;
            }
        }
    }

}

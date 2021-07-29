using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace FortuneVoronoy
{
    public struct Ray
    {
        public PointD EndPoint { get; set; }
        public PointD Direction { get; set; }
    }
    public struct Line
    {
        public PointD EndPoint1 { get; set; }
        public PointD EndPoint2 { get; set; }
    }
    public struct Parabola
    {
        public PointD Focus { get; set; }
        public double AtX(double x, double directrix)
        {
            return (Focus.X - x) * (Focus.X - x) / (2 * (Focus.Y - directrix)) + (Focus.Y + directrix) / 2;
        }
        public double DerivativeAtX(double x, double directrix)
        {
            return (x - Focus.X) / (Focus.Y - directrix);
        }
    }
    public struct Polygon
    {
        public List<PointD> Vertices { get; set; }
        public PointD Site { get; set; }
        public Polygon(PointD site)
        {
            Site = site;
            Vertices = new List<PointD>();
        }
        public override bool Equals(object obj)
        {
            if (obj is Polygon)
            {
                Polygon pol = (Polygon)obj;
                if (pol.Site != Site || Vertices.Count != pol.Vertices.Count) return false;
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (Vertices[i] != pol.Vertices[i]) return false;
                }
                return true;
            }
            return false;
        }
    }
    public struct Seed
    {
        public PointD Point { get; set; }
        public Color Color { get; set; }
        public static implicit operator Seed(PointD p)
        {
            Random r = new Random();
            return new Seed() { Point = p, Color = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)) };
        }
    }
    public class Node
    {
        public bool IsParabola { get; set; }
        public Ray Ray { get; set; }
        public Parabola Parabola { get; set; }
        public Node LeftChildren { get; set; } //Smaller / to the left.
        public Node RightChildren { get; set; } //Larger / to the right.
        public void AssignLeftChildren(Node child)
        {
            LeftChildren = child;
            child.Parent = this;
        }
        public void AssignRightChildren(Node child)
        {
            RightChildren = child;
            child.Parent = this;
        }
        public Node Parent { get; set; }
        public Node(Node parent)
        {
            Parent = parent;
            Exists = true;
        }
        public bool IsLeftChildren { get => Parent?.LeftChildren == this; }
        public bool IsRoot { get; set; }
        public bool Exists { get; set; }
    }
    public interface IEvent
    {
        public PointD AssociatedPoint { get; set; }
    }
    public struct NewSite : IEvent
    {
        public PointD AssociatedPoint { get; set; } //Point of site / focus of parabola.
        public NewSite(PointD p)
        {
            AssociatedPoint = p;
        }
    }
    public struct EdgeEvent : IEvent
    {
        public PointD AssociatedPoint { get; set; } //Point of intersection.
        public Node SquishedParabola { get; set; }
        public EdgeEvent(PointD p, Node n)
        {
            AssociatedPoint = p;
            SquishedParabola = n;
        }
    }
    public struct PointD
    {
        /// <summary>
        /// How many digits of precision to include in the comparison of two points. Set to 3 digits by default.
        /// </summary>
        public int RelevantDigits { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
            RelevantDigits = 3;
        }
        public override bool Equals(object b)
        {
            if (b is PointD)
            {
                PointD pb = (PointD)b;
                if (Math.Abs(X - pb.X) < 1 / Math.Pow(10, RelevantDigits) && Math.Abs(Y - pb.Y) < 1 / Math.Pow(10, RelevantDigits)) return true;
            }
            return false;
        }
        public static bool operator ==(PointD a, PointD b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(PointD a, PointD b)
        {
            return !a.Equals(b);
        }
    }
}

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
        public Line(PointD pt1, PointD pt2)
        {
            EndPoint1 = pt1;
            EndPoint2 = pt2;
        }
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
    public struct Polygon : IEquatable<Polygon>
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
                Console.WriteLine("Passed 1");
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (Vertices[i] != pol.Vertices[i])
                    {
                        Console.WriteLine($"({Vertices[i].X}, {Vertices[i].Y})");
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public bool Equals(Polygon pol)
        {
            Console.WriteLine("Passed 0");
            if (pol.Site != Site || Vertices.Count != pol.Vertices.Count)
            {
                Console.WriteLine(pol.Site != Site);
                Console.WriteLine(Vertices.Count);
                foreach (var item in Vertices)
                {
                    Console.WriteLine($"({item.X}, {item.Y})");
                }
                Console.WriteLine(pol.Vertices.Count);
                return false;
            }
            Console.WriteLine("Passed 1");
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i] != pol.Vertices[i])
                {
                    Console.WriteLine($"({Vertices[i].X}, {Vertices[i].Y})");
                    return false;
                }
            }
            return true;
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
    public class NewPolygon
    {
        public List<Line> Edges { get; set; }
        public PointD Site { get; set; }
        public NewPolygon(PointD site)
        {
            Site = site;
            Edges = new List<Line>();
        }
    }
    public class Vertex
    {
        public PointD Point { get; set; }
        public bool TrueVertex { get; set; } //Whether it is actually a vertex of a polygon or the midpoint between vertices.
        public Vertex(PointD point)
        {
            Point = point;
        }
    }
}

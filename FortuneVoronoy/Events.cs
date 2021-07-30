using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortuneVoronoy
{
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
        public List<Node> SquishedParabolas { get; set; }
        public EdgeEvent(PointD p, Node n)
        {
            AssociatedPoint = p;
            SquishedParabolas = new List<Node>();
            SquishedParabolas.Add(n);
        }
    }
}

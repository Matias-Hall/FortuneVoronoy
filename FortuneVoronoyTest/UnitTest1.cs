using Microsoft.VisualStudio.TestTools.UnitTesting;
using FortuneVoronoy;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FortuneVoronoyTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [Timeout(5_000)]
        public void TestMethod1()
        {
            FortuneVoronoy.FortuneVoronoy fortune = new FortuneVoronoy.FortuneVoronoy();
            List<Seed> seeds = new List<Seed>() { new PointD(9.8, 21.85), new PointD(22.9, 11.04), new PointD(34.1, 27.1) };
            fortune.Run(seeds);
            fortune.events.Values.ToList().ForEach(x => Console.WriteLine($"P:({x.AssociatedPoint.X},{x.AssociatedPoint.Y})"));
            Assert.IsTrue(fortune.events.Values.Where(x => ComparePoints(x.AssociatedPoint, new PointD(22.163, 23.489), 3)).Any());
        }
        private bool ComparePoints(PointD a, PointD b, int digits)
        {
            if (Math.Abs(a.X - b.X) < 1 / Math.Pow(10, digits) && Math.Abs(a.Y - b.Y) < 1 / Math.Pow(10, digits)) return true;
            return false;
        }
    }
}

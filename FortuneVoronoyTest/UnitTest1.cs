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
            List<Seed> seeds = new List<Seed>() { new PointD(25.74, 12.75), new PointD(20.6, 2.9), new PointD(32.1, 24.2), new PointD(36.8, 7.15), new PointD(13.3, 17.5) };
            List<Polygon> p = fortune.Run(seeds);
            List<PointD> shouldContain = new List<PointD>() { new PointD(28.726, 4.926), new PointD(17.802, 10.626), new PointD(34.123, 15.585), new PointD(22.22, 22.196), };
            Polygon completed = p.Where(x => x.Site == new PointD(25.74, 12.75)).First();
            foreach (var cont in shouldContain)
            {
                Assert.IsTrue(completed.Vertices.Contains(cont));
            }
        }
        private bool ComparePoints(PointD a, PointD b, int digits)
        {
            if (Math.Abs(a.X - b.X) < 1 / Math.Pow(10, digits) && Math.Abs(a.Y - b.Y) < 1 / Math.Pow(10, digits)) return true;
            return false;
        }
    }
}

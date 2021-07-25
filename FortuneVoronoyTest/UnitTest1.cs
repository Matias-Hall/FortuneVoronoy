using Microsoft.VisualStudio.TestTools.UnitTesting;
using FortuneVoronoy;
using System.Collections.Generic;
using System.Linq;

namespace FortuneVoronoyTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            FortuneVoronoy.FortuneVoronoy fortune = new FortuneVoronoy.FortuneVoronoy();
            List<Seed> seeds = new List<Seed>() { new PointD(9.8, 21.85), new PointD(22.9, 11.04), new PointD(34.1, 27.1) };
            fortune.Run(seeds);
            CollectionAssert.Contains(fortune.events.Values.Select(x => x.AssociatedPoint).ToList(), new PointD(22.163, 23.489));
        }
    }
}

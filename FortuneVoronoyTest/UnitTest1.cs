using Microsoft.VisualStudio.TestTools.UnitTesting;
using FortuneVoronoy;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace FortuneVoronoyTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [Timeout(5_000)]
        public void GeneralTest1()
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
        [TestMethod]
        [Timeout(5_000)]
        public void GeneralTest2()
        {
            FortuneVoronoy.FortuneVoronoy fortune = new FortuneVoronoy.FortuneVoronoy();
            List<Seed> seeds = new List<Seed>() { new PointD(844, 330), new PointD(1119, 235), new PointD(1278, 541), new PointD(507, 674), new PointD(428, 464), new PointD(559, 313), new PointD(1078, 184), new PointD(1625, 839), new PointD(741, 889) };
            List<Polygon> p = fortune.Run(seeds);
            Polygon p1 = new Polygon(new PointD(1078, 184));
            p1.Vertices = new List<PointD>() { new PointD(727.6555672545406, -116.99039220847601), new PointD(987.1936383928571, 298.98158482142856), new PointD(17553.635316698692, -13019.13819577738) };
            Polygon p2 = new Polygon(new PointD(1119, 235));
            p2.Vertices = new List<PointD>() { new PointD(987.1936383928571, 298.98158482142856), new PointD(1045.422925797189, 467.5400483602842), new PointD(2123.7180612244897, -92.75056122448984), new PointD(17553.635316698692, -13019.13819577738)};
            Polygon p3 = new Polygon(new PointD(559, 313));
            p3.Vertices = new List<PointD>() { new PointD(727.6555672545406, -116.99039220847601), new PointD(630.7626334339107, 507.582152184386), new PointD(689.8923136967687, 516.099446848288) };
            Polygon p4 = new Polygon(new PointD(844, 330));
            p4.Vertices = new List<PointD>() { new PointD(727.6555672545406, -116.99039220847601), new PointD(987.1936383928571, 298.98158482142856), new PointD(1045.422925797189, 467.5400483602842), new PointD(689.8923136967687, 516.099446848288), new PointD(783.5494798987911, 607.8507986217809), new PointD(961.2854800086252, 640.6000079443443) };
            Polygon p5 = new Polygon(new PointD(428, 464));
            p5.Vertices = new List<PointD>() { new PointD(630.7626334339107, 507.582152184386), };
            Polygon p6 = new Polygon(new PointD(1278, 541));
            p6.Vertices = new List<PointD>() { new PointD(1045.422925797189, 467.5400483602842), new PointD(2123.7180612244897, -92.75056122448984), new PointD(961.2854800086252, 640.6000079443443), new PointD(1190.3575763403635, 994.0819496976303) };
            Polygon p7 = new Polygon(new PointD(507, 674));
            p7.Vertices = new List<PointD>() { new PointD(630.7626334339107, 507.582152184386), new PointD(689.8923136967687, 516.099446848288), new PointD(783.5494798987911, 607.8507986217809), };
            Polygon p8 = new Polygon(new PointD(1625, 839));
            p8.Vertices = new List<PointD>() { new PointD(2123.7180612244897, -92.75056122448984), new PointD(1190.3575763403635, 994.0819496976303), new PointD(17553.635316698692, -13019.13819577738) };
            Polygon p9 = new Polygon(new PointD(741, 889));
            p9.Vertices = new List<PointD>() { new PointD(783.5494798987911, 607.8507986217809), new PointD(961.2854800086252, 640.6000079443443), new PointD(1190.3575763403635, 994.0819496976303), };
            List<Polygon> polys = new List<Polygon>() { p1, p2, p3, p4, p5, p6, p7, p8, p9 };
            foreach (var pol in polys)
            {
                Assert.IsTrue(p.Contains(pol), $"Polygon not correct: {pol.Site.X}, {pol.Site.Y}");
            }
        }
        [TestMethod]
        [Timeout(5_000)]
        public void GeneralTest3()
        {
            FortuneVoronoy.FortuneVoronoy fortune = new FortuneVoronoy.FortuneVoronoy();
            List<Seed> seeds = new List<Seed>() { new PointD(559, 420), new PointD(208, 164), new PointD(127, 579), new PointD(541, 283), new PointD(28, 529), new PointD(250, 84), new PointD(65, 428), new PointD(339, 352), new PointD(187, 537), new PointD(554, 69), new PointD(348, 80), new PointD(248, 239), new PointD(245, 164), new PointD(307, 264), new PointD(64, 360), new PointD(421, 239), new PointD(357, 37), new PointD(66, 158), new PointD(235, 292), new PointD(226, 354), new PointD(292, 177), new PointD(232, 471), new PointD(94, 293), new PointD(183, 216), new PointD(258, 346), new PointD(156, 84), new PointD(256, 509), new PointD(81, 25), new PointD(273, 457), new PointD(275, 207), new PointD(84, 311), new PointD(311, 283), new PointD(431, 292), new PointD(455, 453), new PointD(450, 397), new PointD(198, 195), new PointD(51, 218), new PointD(456, 64), new PointD(21, 535), new PointD(95, 392), new PointD(426, 530), new PointD(11, 342), new PointD(438, 487), new PointD(547, 45), new PointD(510, 268), new PointD(442, 20), new PointD(150, 599), new PointD(514, 527), new PointD(287, 26), new PointD(388, 229), };
            List<Polygon> p = fortune.Run(seeds);
            StringBuilder sb = new StringBuilder();
            foreach (var a in p)
            {
                foreach (var b in a.Vertices)
                {
                    sb.Append($"new Point({Math.Round(b.X)}, {Math.Round(b.Y)})");
                }
            }
            Console.WriteLine(sb);
            Assert.Inconclusive();
        }
        [TestMethod]
        [Timeout(5_000)]
        public void SameEventKeyTest()
        {
            FortuneVoronoy.FortuneVoronoy fortune = new FortuneVoronoy.FortuneVoronoy();
            List<Seed> seeds = new List<Seed>() { new PointD(6, 6), new PointD(15, 6), new PointD(8, 10), new PointD(7, 14), new PointD(15, 14), };
            List<Polygon> p = fortune.Run(seeds);
            List<PointD> shouldContain = new List<PointD>() { new PointD(10.5, 6.25), new PointD(12.642, 10), new PointD(1.8334, 10.583), new PointD(11, 12.875), };
            Polygon completed = p.Where(x => x.Site == new PointD(8, 10)).First();
            completed.Vertices.ForEach(x => Console.WriteLine($"{x.X}, {x.Y}"));
            foreach (var cont in shouldContain)
            {
                Assert.IsTrue(completed.Vertices.Contains(cont));
            }
        }
    }
}

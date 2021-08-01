using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FortuneVoronoy;
using System.Diagnostics;

namespace FortuneVoronoyTest
{
    [TestClass]
    public class PerformanceTest
    {
        [TestMethod]
        public void Performance()
        {
            Stopwatch s = new Stopwatch();
            int[] tests = { 10, 100, 1000, 2000, 5000, 10000 };
            foreach (int t in tests)
            {
                List<Seed> seeds = RandomSeeds(t);
                FortuneVoronoy.FortuneVoronoy f = new FortuneVoronoy.FortuneVoronoy();
                s.Start();
                f.Run(seeds);
                s.Stop();
                Console.WriteLine($"{t} seeds result: {s.ElapsedMilliseconds} milliseconds");
                s.Reset();
            }
        }
        private List<Seed> RandomSeeds(int n)
        {
            HashSet<Seed> seeds = new HashSet<Seed>();
            Random r = new Random();
            for (int i = 0; i < n; i++)
            {
                bool added = seeds.Add(new Seed() { Point = new PointD(r.Next(0, 5000), r.Next(0, 5000)) });
                while (!added) //Avoids duplicates
                {
                    added = seeds.Add(new Seed() { Point = new PointD(r.Next(0, 5000), r.Next(0, 5000)) });
                }
            }
            return seeds.ToList();
        }
    }
}

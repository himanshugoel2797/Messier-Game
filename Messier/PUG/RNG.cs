using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.PUG
{
    public class RNG
    {
        private const double Prime = 7243469162906133262707138361729247674528418426076702186281286038623238274842547507072974617594640311d;
        private const double Root =  3242736143229285405697273596419677873912657748731448981302390864459158863881443495029809033284732127d;

        private static double seed = 1;

        private static void UpdateSeed()
        {
            seed = ((Math.PI * Root * seed) % Prime);
        }

        public static double NextDouble()
        {
            UpdateSeed();
            return seed/10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d;
        }

        public static double NextLargeNum()
        {
            UpdateSeed();
            return seed;
        }

        public static int NextInt()
        {
            UpdateSeed();
            return (int)(seed % 100000000);
        }
    }
}

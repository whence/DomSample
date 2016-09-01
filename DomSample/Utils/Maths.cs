using System;
using System.Security.Cryptography;

namespace DomSample.Utils
{
    public static class Maths
    {
        public static int StepInLoop(int current, int step, int min, int max)
        {
            int stepDirection = Math.Sign(step);
            int stepAbsolute = Math.Abs(step);
            
            for (int i = 0; i < stepAbsolute; i++)
            {
                current += stepDirection;
                if (current > max)
                    current = min;

                if (current < min)
                    current = max;
            }

            return current;
        }

        public static int RandomInt32()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                return RandomInt32(provider);   
            }
        }

        private static int RandomInt32(RandomNumberGenerator generator)
        {
            var bytes = new byte[4];
            generator.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        //reference from http://msdn.microsoft.com/en-us/magazine/cc163957.aspx
        public static long Choose(long n, long k)
        {
            if (n < 0 || k < 0)
                throw new Exception("Invalid negative parameter in Choose()");
            if (n < k) return 0;
            if (n == k) return 1;

            long delta, iMax;

            if (k < n - k) // ex: Choose(100,3)
            {
                delta = n - k;
                iMax = k;
            }
            else         // ex: Choose(100,97)
            {
                delta = k;
                iMax = n - k;
            }

            long ans = delta + 1;

            for (long i = 2; i <= iMax; ++i)
            {
                checked { ans = (ans * (delta + i)) / i; }
            }

            return ans;
        }
    }
}

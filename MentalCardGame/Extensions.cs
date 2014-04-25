using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    internal static class Extensions
    {
        public static int Jacobi(this BigInteger a, BigInteger b)
        {
            // http://2000clicks.com/MathHelp/NumberTh27JacobiSymbolAlgorithm.aspx
            if (b <= 0 || (b % 2) == 0)
                return 0;
            var j = 1;
            if (a < 0)
            {
                a = -a;
                if ((b % 4) == 3)
                    j = -j;
            }

            while (a != 0)
            {
                while ((a % 2) == 0)
                {
                    /* Process factors of 2: Jacobi(2,b)=-1 if b=3,5 (mod 8) */
                    a = a / 2;
                    if ((b % 8) == 3 || (b % 8) == 5)
                        j = -j;
                }

                /* Quadratic reciprocity: Jacobi(a,b)=-Jacobi(b,a) if a=3,b=3 (mod 4) */
                //  vertausche a und b
                var t = a;
                a = b;
                b = t;
                if ((a % 4) == 3 && (b % 4) == 3)
                    j = -j;
                a = a % b;
            }

            if (b == 1)
                return j;
            else
                return 0;
        }
    }
}
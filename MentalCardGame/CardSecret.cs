using System;
using System.Collections.Generic;
using System.Diagnostics.Debug;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    internal class CardSecret
    {
        public readonly BigInteger[,] r;
        public readonly int[,] b;
        public readonly int k;
        public readonly int w;
        private CardEngine engine;

        public CardSecret(CardEngine engine)
        {
            this.w = engine.W;
            this.k = engine.K;
            r = new BigInteger[k, w];
            b = new int[k, w];
            this.engine = engine;
        }

        public static CardSecret Combine(CardSecret cs1, CardSecret cs2)
        {
            Assert(cs1.engine == cs2.engine);
            var newcs = new CardSecret(cs1.engine);
            for (int i = 0; i < newcs.k; i++)
            {
                for (int j = 0; j < newcs.w; j++)
                {
                    newcs.r[i, j] = cs1.r[i, j] * cs2.r[i, j];
                    newcs.r[i, j] = newcs.r[i, j] % cs1.engine.Ring[i].M;
                    if (cs1.b[i, j] == 1 & cs2.b[i, j] == 1)
                        newcs.r[i, j] *= cs1.engine.Ring[i].Y;
                    newcs.r[i, j] = newcs.r[i, j] % cs1.engine.Ring[i].M;
                    newcs.b[i, j] = cs2.b[i, j] ^ cs1.b[i, j];
                }
            }

            return newcs;
        }
    }
}
using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame.RNG
{
    public class RNG : ISecureRNG
    {
        public uint Next()
        {
            return Windows.Security.Cryptography.CryptographicBuffer.GenerateRandomNumber();
        }

        public void TwoPrimes(out BigInteger p, out BigInteger q)
        {
            var g = new PrivateKey();
            p = new BigInteger(g.P.Reverse().Concat((byte)0).ToArray());
            q = new BigInteger(g.Q.Reverse().Concat((byte)0).ToArray());
        }
    }
}
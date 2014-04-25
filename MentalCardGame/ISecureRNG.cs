using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MentalCardGame
{
    public interface ISecureRNG
    {
        uint Next();

        void TwoPrimes(out BigInteger p, out BigInteger q);
    }
}
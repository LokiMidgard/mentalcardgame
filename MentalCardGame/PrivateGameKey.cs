using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    public class PrivateGameKey
    {
        internal BigInteger P
        {
            get;
            private set;
        }

        internal BigInteger Q
        {
            get;
            private set;
        }

        internal BigInteger Y
        {
            get;
            private set;
        }

        internal BigInteger M
        {
            get;
            private set;
        }

        public int Playernumber
        {
            get;
            private set;
        }

        internal PrivateGameKey(int playernumber, ISecureRNG rng)
        {
            this.Playernumber = playernumber;
            //var key = new PrivateKey();
            //var p = new BigInteger(key.P.Reverse().Concat((byte)0).ToArray());
            //var q = new BigInteger(key.Q.Reverse().Concat((byte)0).ToArray());
            rng.TwoPrimes(out var p, out var q);
            var m = p * q;
            BigInteger y = 1;
            do
            {
                y++;
            }
            while ((y.Jacobi(m) != 1) || ((y.Jacobi(p) == 1) && (y.Jacobi(q) == 1)));
            Q = q;
            P = p;
            Y = y;
            M = m;
        }

        public static implicit operator PublicGameKey(PrivateGameKey k)
        {
            return new PublicGameKey(k.Y, k.M, k.Playernumber);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                result = result * prime + P.GetHashCode();
                result = result * prime + Q.GetHashCode();
                result = result * prime + Y.GetHashCode();
                result = result * prime + M.GetHashCode();
                result = result * prime + Playernumber.GetHashCode();
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PrivateGameKey))
                return false;
            var other = (PrivateGameKey)obj;
            if (!P.Equals(other.P))
                return false;
            if (!Q.Equals(other.Q))
                return false;
            if (!Y.Equals(other.Y))
                return false;
            if (!M.Equals(other.M))
                return false;
            if (!Playernumber.Equals(other.Playernumber))
                return false;
            return true;
        }
    }
}
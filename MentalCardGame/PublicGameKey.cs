using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    public class PublicGameKey
    {
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

        internal PublicGameKey(BigInteger y, BigInteger m, int playernumber)
        {
            M = m;
            Y = y;
            this.Playernumber = playernumber;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                result = result * prime + Y.GetHashCode();
                result = result * prime + M.GetHashCode();
                result = result * prime + Playernumber.GetHashCode();
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PublicGameKey))
                return false;
            var other = (PublicGameKey)obj;
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
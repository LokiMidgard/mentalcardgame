using System;
using System.Collections.Generic;
using System.Diagnostics.Debug;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    [System.Diagnostics.DebuggerDisplay("Card({Type}) {GetHashCode()}")]
    public class Card
    {
        private readonly CardEngine engine;

        private Card createdFrom;

        private CardSecret createdFromSecret;

        private int? type;

        internal Card(CardEngine engine)
        {
            this.w = engine.W;
            this.k = engine.K;
            this.engine = engine;
            values = new BigInteger[k, w];
        }

        internal Card(int type, CardEngine engine) : this(engine)
        {
            Assert(type < Math.Pow(2, w), "Value of Card is to High");

            this.type = type;

            for (int j = 0; j < this.w; j++)
            {
                if ((type & 1) == 1)
                {
                    values[0, j] = engine.Ring[0].Y;
                    type--;
                    type /= 2;
                }
                else
                {
                    values[0, j] = 1;
                    type /= 2;
                }
            }

            for (int i = 1; i < this.k; i++)
                for (int j = 0; j < this.w; j++)
                    values[i, j] = 1;
        }

        public int? Type
        {
            get
            {
                if (type.HasValue)
                    return type.Value;

                var calc = CalculateType();
                if (calc != -1)
                    return calc;

                if (createdFrom != null)
                    return createdFrom.Type;

                return null;
            }
        }

        internal int k { get; private set; }

        internal BigInteger[,] values { get; private set; }

        internal int w { get; private set; }

        public static bool operator !=(Card c1, Card c2)
        {
            return !(c1 == c2);
        }

        public static bool operator ==(Card c1, Card c2)
        {
            if (((object)c1) == null)
                return ((object)c2) == null;
            return c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Card))
                return false;
            var other = (Card)obj;
            if (!values.OfType<BigInteger>().SequenceEqual(other.values.OfType<BigInteger>()))
                return false;
            if (!k.Equals(other.k))
                return false;
            if (!w.Equals(other.w))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                for (int i = 0; i < k; i++)
                    for (int j = 0; j < w; j++)
                        result = result * prime + values[i, j].GetHashCode();
                result = result * prime + k.GetHashCode();
                result = result * prime + w.GetHashCode();
                return result;
            }
        }

        public Card MaskCard()
        {
            return MaskCard(CreateSecret());
        }

        public ProveCreatedFromPhase1Output ProveCreatedFrom(int s)
        {
            var cl = new Card[s];
            for (int i = 0; i < s; i++)
            {
                cl[i] = MaskCard();
            }

            return new ProveCreatedFromPhase1Output() { Cl = cl, C = this.createdFrom };
        }

        public ProveCreatedFromPhase2Output ProveCreatedFromPhase2(ProveCreatedFromPhase1Output input, Card c)
        {
            if (input.C != c)
                throw new ArgumentException("Cards To test are different");
            // Choose random subset X ⊆ {1,...,s}
            var x = input.Cl.Select(y => (GenerateNumber() & 1) == 1).ToArray();
            return new ProveCreatedFromPhase2Output() { X = x };
        }

        public ProveCreatedFromPhase3Output ProveCreatedFromPhase3(ProveCreatedFromPhase2Output phase2, ProveCreatedFromPhase1Output phase1)
        {
            Assert(phase2.X.Length == phase1.Cl.Length);
            var publish = new CardSecret[phase1.Cl.Length];
            for (int i = 0; i < phase1.Cl.Length; i++)
            {
                if (phase2.X[i])
                {
                    publish[i] = phase1.Cl[i].createdFromSecret;
                }
                else
                {
                    publish[i] = CardSecret.Combine(phase1.Cl[i].createdFromSecret, this.createdFromSecret);
                }
            }

            return new ProveCreatedFromPhase3Output() { Secrets = publish };
        }

        public bool ProveCreatedFromPhase4(ProveCreatedFromPhase3Output phase3, ProveCreatedFromPhase2Output phase2, ProveCreatedFromPhase1Output phase1)
        {
            Assert(phase3.Secrets.Length == phase2.X.Length);
            for (int i = 0; i < phase3.Secrets.Length; i++)
            {
                if (phase2.X[i])
                {
                    var ccc = this.MaskCard(phase3.Secrets[i]);
                    if (ccc != phase1.Cl[i])
                        return false;
                }
                else
                {
                    var ccc = phase1.C.MaskCard(phase3.Secrets[i]);
                    if (ccc != phase1.Cl[i])
                        return false;
                }
            }

            return true;
        }

        public UncoverdSecrets UncoverSecrets()
        {
            var index = engine.PrivateKey.Playernumber;
            Assert(k > index);
            int[] b = new int[w];
            for (int j = 0; j < this.w; j++)
            {
                if (Qr(values[index, j], engine.PrivateKey.P, engine.PrivateKey.Q))
                    b[j] = 0;
                else
                    b[j] = 1;
            }

            return new UncoverdSecrets() { B = b, PlayerIndex = index };
        }

        public int UnmaskCard(params UncoverdSecrets[] b)
        {
            var cs = new CardSecret(engine);

            if (b.Length == engine.Ring.Length - 1)
                b = b.Concat(this.UncoverSecrets()).OrderBy(x => x.PlayerIndex).ToArray();
            else
                b = b.OrderBy(x => x.PlayerIndex).ToArray();

            if (b.Select(x => x.PlayerIndex).Distinct().Count() != engine.K)
                throw new ArgumentException("UncoverSecrets are needed from all Players. We head only " + b.Length + " including our own.");

            for (int i = 0; i < k; i++)
                for (int j = 0; j < this.w; j++)
                {
                    cs.b[i, j] = b[i].B[j];
                    cs.r[i, j] = 0;
                }

            int type = 0;
            int p2 = 1;
            for (int j = 0; j < this.w; j++)
            {
                bool bit = false;
                for (int i = 0; i < this.k; i++)
                {
                    if ((cs.b[i, j] & 1) == 1)
                        bit = !bit;
                }

                if (bit)
                    type += p2;
                p2 *= 2;
            }

            Assert(type < engine.MaxNumberOfCard, "Somehow we Uncoverd a Card that is Higher then the Maximum of Cards that are Valid");

            this.type = type;

            return type;
        }

        internal CardSecret CreateSecret()
        {
            var cs = new CardSecret(engine);
            for (int i = 0; i < this.k; i++)
            {
                for (int j = 0; j < this.w; j++)
                {
                    // wähle r aus Z^*_m
                    cs.r[i, j] = GenerateNumberInZStar_m(engine.Ring[i].M);
                    //random 1 oder 0 für b
                    // bei der player spalte von index auf 0 setzen
                    if (i != engine.PrivateKey.Playernumber)
                        cs.b[i, j] = (int)GenerateNumber() & 1;
                    else
                        cs.b[engine.PrivateKey.Playernumber, j] = 0;
                }
            }

            // XOR b_{ij} with i \neq index (keep type of this card)
            for (int i = 0; i < this.k; i++)
            {
                if (i == engine.PrivateKey.Playernumber)
                    continue;
                for (int j = 0; j < this.w; j++)
                    cs.b[engine.PrivateKey.Playernumber, j] ^= cs.b[i, j];
            }

            return cs;
        }

        internal Card MaskCard(CardSecret cs)
        {
            Card erg = new Card(engine);

            for (int i = 0; i < this.k; i++)
                for (int j = 0; j < this.w; j++)
                    erg.values[i, j] = MaskNumber(engine.Ring[i], this.values[i, j], cs.r[i, j], cs.b[i, j]);

            erg.createdFrom = this;
            erg.createdFromSecret = cs;

            return erg;
        }

        private int CalculateType()
        {
            var p2 = 1;
            var t = 0;
            for (int i = 0; i < k; i++)
                for (int j = 0; j < w; j++)
                {
                    if (i == 0)
                    {
                        if (values[i, j] != 1 && values[i, j] != this.engine.Ring[0].Y)
                            return -1;

                        if (values[i, j] != 1)
                            t += p2;
                        p2 *= 2;
                    }
                    else
                    {
                        if (values[i, j] != 1)
                            return -1;
                    }
                }
            return t;
        }

        private uint GenerateNumber()
        {
            uint t;
            t = engine.Rng.Next();
            return t;
        }

        private uint GenerateNumberInZStar_m(BigInteger m)
        {
            uint t;
            do
            {
                t = GenerateNumber();
            }
            while (BigInteger.GreatestCommonDivisor(m, t) != 1 || t == 1);
            return t;
        }

        private BigInteger MaskNumber(PublicGameKey key, BigInteger z, BigInteger r, int b)
        {
            Assert(b == 1 || b == 0, "b muss 1 oder 0 sein. War " + b);
            // berechne  z * r^2 * y^b mod m
            BigInteger erg = r * r;
            if (b == 1)
                erg *= key.Y;
            erg *= z;
            erg %= key.M;
            return erg;
        }

        private bool Qr(BigInteger a, BigInteger p, BigInteger q)
        {
            return ((a.Jacobi(p) == 1) && (a.Jacobi(q) == 1));
        }

        public class ProveCreatedFromPhase1Output
        {
            public ProveCreatedFromPhase1Output()
            {
            }

            public Card C { get; internal set; }

            public Card[] Cl { get; internal set; }
        }

        public class ProveCreatedFromPhase2Output
        {
            internal ProveCreatedFromPhase2Output()
            {
            }

            internal bool[] X { get; set; }
        }

        public class ProveCreatedFromPhase3Output
        {
            internal ProveCreatedFromPhase3Output()
            {
            }

            internal CardSecret[] Secrets { get; set; }
        }

        public class UncoverdSecrets
        {
            internal UncoverdSecrets()
            {
            }

            internal int[] B { get; set; }

            internal int PlayerIndex { get; set; }
        }
    }
}
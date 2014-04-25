using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    public class Stack : List<Card>
    {
        private readonly CardEngine engine;

        private Stack createdFrom;
        private StackSecret createdFromSecret;

        internal Stack(CardEngine engine) : this(engine, new Card[0])
        {
        }

        internal Stack(CardEngine engine, params Card[] cards) : this(engine, (IEnumerable<Card>)cards)
        {
        }

        internal Stack(CardEngine engine, IEnumerable<Card> cards) : base(cards)
        {
            this.engine = engine;
        }

        public Stack Shuffle()
        {
            var secrete = new StackSecret();

            var permutation = GenereatePermutation();

            for (int i = 0; i < permutation.Length; i++)
            {
                var s = this[permutation[i]].CreateSecret();
                secrete.Add(Tuple.Create(permutation[i], s));
            }

            return Shuffle(secrete);
        }

        internal Stack Shuffle(StackSecret secrete)
        {
            var erg = new Stack(engine);

            erg.createdFrom = this;
            erg.createdFromSecret = secrete;
            for (int i = 0; i < secrete.Count; i++)
            {
                var s = secrete[i];
                erg.Add(this[s.Item1].MaskCard(s.Item2));
            }

            return erg;
        }

        public ProveShufflePhase1Output ProveShuffle(int s)
        {
            var stackl = new Stack[s];
            for (int i = 0; i < s; i++)
            {
                stackl[i] = this.Shuffle();
            }

            return new ProveShufflePhase1Output() { Sl = stackl, S = createdFrom };
        }

        public ProveShufflePhase2Output ProveShufflePhase2(Stack original, ProveShufflePhase1Output phase1)
        {
            if (original != phase1.S)
                throw new ArgumentException("Stacks to test are Not Equal");
            var x = phase1.Sl.Select(y => (engine.Rng.Next() & 1) == 1).ToArray();

            return new ProveShufflePhase2Output() { X = x };
        }

        public ProveShufflePhase3Output ProveShufflePhase3(ProveShufflePhase1Output phase1, ProveShufflePhase2Output phase2)
        {
            var publish = new StackSecret[phase1.Sl.Length];
            System.Diagnostics.Debug.Assert(phase2.X.Length == phase1.Sl.Length, "X should be the same size as S_l");
            for (int i = 0; i < phase2.X.Length; i++)
            {
                if (phase2.X[i])
                {
                    publish[i] = phase1.Sl[i].createdFromSecret;
                }
                else
                {
                    publish[i] = StackSecret.Combine(this.createdFromSecret, phase1.Sl[i].createdFromSecret);
                }
            }

            return new ProveShufflePhase3Output() { Secrets = publish };
        }

        public bool ProveShufflePhase4(ProveShufflePhase1Output phase1, ProveShufflePhase2Output phase2, ProveShufflePhase3Output phase3)
        {
            for (int i = 0; i < phase2.X.Length; i++)
            {
                if (phase2.X[i])
                {
                    var s = this.Shuffle(phase3.Secrets[i]);
                    if (s != phase1.Sl[i])
                        return false;
                }
                else
                {
                    var s = phase1.S.Shuffle(phase3.Secrets[i]);
                    if (s != phase1.Sl[i])
                        return false;
                }
            }

            return true;
        }

        private int[] GenereatePermutation()
        {
            var start = Enumerable.Range(0, this.Count).ToList();
            var erg = new List<int>();
            while (start.Any())
            {
                var index = (int)(engine.Rng.Next() % start.Count);
                erg.Add(start[index]);
                start.RemoveAt(index);
            }
            return erg.ToArray();
        }

        public static bool operator !=(Stack c1, Stack c2)
        {
            return !(c1 == c2);
        }

        public static bool operator ==(Stack c1, Stack c2)
        {
            if (((object)c1) == null)
                return ((object)c2) == null;
            return c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Stack))
                return false;
            if (Object.ReferenceEquals(this, obj))
                return true;
            var other = (Stack)obj;
            if (this.Count != other.Count)
                return false;
            for (int i = 0; i < Count; i++)
                if (!this[i].Equals(other[i]))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                foreach (var item in this)
                    result = result * prime + item.GetHashCode();
                return result;
            }
        }

        public class ProveShufflePhase1Output
        {
            internal ProveShufflePhase1Output()
            {
            }

            internal Stack S { get; set; }

            internal Stack[] Sl { get; set; }
        }

        public class ProveShufflePhase2Output
        {
            internal ProveShufflePhase2Output()
            {
            }

            internal bool[] X { get; set; }
        }

        public class ProveShufflePhase3Output
        {
            internal ProveShufflePhase3Output()
            {
            }

            internal StackSecret[] Secrets { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MentalCardGame
{
    public class CardEngine
    {
        private readonly PrivateGameKey privateKey;
        private readonly PublicGameKey[] ring;
        private readonly int maxNumberOfCard;
        private readonly ISecureRNG rng;

        public int MaxNumberOfCard { get { return maxNumberOfCard; } }

        public int NumberOfPlayers { get { return ring.Length; } }

        internal int W { get { return (int)Math.Ceiling(Math.Log(maxNumberOfCard, 2)); } }

        internal int K { get { return NumberOfPlayers; } }

        internal ISecureRNG Rng { get { return rng; } }

        internal PublicGameKey[] Ring { get { return ring; } }

        internal PrivateGameKey PrivateKey { get { return privateKey; } }

        public CardEngine(PrivateGameKey ownKey, ISecureRNG rng, int maxCardNumber, params PublicGameKey[] keyRing)
        {
            if (keyRing.Select(x => x.Playernumber).Distinct().Count() != keyRing.Count())
                throw new ArgumentException("No two Pubick keys in keyRing may have the same PlayerNumber");

            if (!keyRing.Any(x => x.Equals((PublicGameKey)ownKey)))
                throw new ArgumentException("The Public Key of your Private Key must be containt in keyRing");
            for (int i = 0; i < keyRing.Count(); i++)
            {
                if (keyRing.Where(x => x.Playernumber == i).Count() != 1)
                    throw new ArgumentException("The Key of each Player must be exactly one time in the KeyRing.");
            }

            this.maxNumberOfCard = maxCardNumber;
            this.ring = keyRing.OrderBy(x => x.Playernumber).ToArray();
            this.privateKey = ownKey;
            this.rng = rng;
        }

        public static PrivateGameKey CreateGameKey(int playernumber, ISecureRNG rng)
        {
            if (playernumber < 0)
                throw new ArgumentException("Playernumber must be >= 0");
            return new PrivateGameKey(playernumber, rng);
        }

        public Card CreateCard(int number)
        {
            if (number < 0 || number > MaxNumberOfCard)
                throw new ArgumentException("number must was " + number + ". Must be 0 < number < MaxNumber");

            return new Card(number, this);
        }

        public Stack CreateStack(params Card[] cards)
        {
            return new Stack(this, cards);
        }

        #region Serialisation

        private StackSecret DeSerializeStackSeret(XElement root)
        {
            var erg = new StackSecret();

            var x = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "Secrets"));

            erg.AddRange(x.Nodes().OfType<XElement>().Select(v =>
            {
                return Tuple.Create(
                        int.Parse(v.Nodes().OfType<XElement>().Single(z => z.Name.LocalName == "Index").Value),
                        DeSerializeCardSeret(v.Nodes().OfType<XElement>().Single(z => z.Name.LocalName == "CardSecret"))
                    );
            }));
            return erg;
        }

        private XElement SerializeStackSecret(StackSecret stackSecret)
        {
            var root = new XElement(XName.Get("StackSecret"));

            var x = new XElement(XName.Get("Secrets"));

            for (int i = 0; i < stackSecret.Count; i++)
            {
                var v = new XElement(XName.Get("Secret"));
                var v1 = new XElement(XName.Get("Index"), stackSecret[i].Item1);
                var v2 = SerializeCardSecret(stackSecret[i].Item2);
                x.Add(v);
                v.Add(v1, v2);
            }
            root.Add(x);

            return root;
        }

        public Stack.ProveShufflePhase3Output DeSerializeProveShufflePhase3Output(XElement root)
        {
            var erg = new Stack.ProveShufflePhase3Output();

            var x = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "Secrets"));

            erg.Secrets = x.Nodes().OfType<XElement>().Select(v => DeSerializeStackSeret(v)).ToArray();
            return erg;
        }

        public XElement SerializeProveShufflePhase3Output(Stack.ProveShufflePhase3Output prove)
        {
            var root = new XElement(XName.Get("ProveShufflePhase3Output"));

            var x = new XElement(XName.Get("Secrets"));

            for (int i = 0; i < prove.Secrets.Length; i++)
            {
                var v = SerializeStackSecret(prove.Secrets[i]);
                x.Add(v);
            }
            root.Add(x);

            return root;
        }

        public Stack.ProveShufflePhase2Output DeSerializeProveShufflePhase2Output(XElement root)
        {
            var erg = new Stack.ProveShufflePhase2Output();

            var x = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "X"));

            erg.X = x.Nodes().OfType<XElement>().Select(v => bool.Parse(v.Value)).ToArray();
            return erg;
        }

        public XElement SerializeProveShufflePhase2Output(Stack.ProveShufflePhase2Output prove)
        {
            var root = new XElement(XName.Get("ProveShufflePhase2Output"));

            var x = new XElement(XName.Get("X"));

            for (int i = 0; i < prove.X.Length; i++)
            {
                var v = new XElement(XName.Get("Stack"), prove.X[i].ToString());
                x.Add(v);
            }
            root.Add(x);

            return root;
        }

        public Stack.ProveShufflePhase1Output DeSerializeProveShufflePhase1Output(XElement root)
        {
            var erg = new Stack.ProveShufflePhase1Output();

            var c = DeSerializeStack(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "Stack").FirstNode as XElement);
            var cl = (root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "Sl"));

            erg.S = c;
            erg.Sl = cl.Nodes().OfType<XElement>().Select(x => DeSerializeStack(x)).ToArray();
            return erg;
        }

        public XElement SerializeProveShufflePhase1Output(Stack.ProveShufflePhase1Output prove)
        {
            var root = new XElement(XName.Get("ProveShufflePhase1Output"));

            var c = new XElement(XName.Get("Stack"), SerializeStack(prove.S));
            var cl = new XElement(XName.Get("Sl"));

            for (int i = 0; i < prove.Sl.Length; i++)
            {
                var v = SerializeStack(prove.Sl[i]);
                cl.Add(v);
            }
            root.Add(c, cl);

            return root;
        }

        public Stack DeSerializeStack(XElement root)
        {
            var erg = new Stack(this);

            var b = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "Cards"));

            erg.AddRange(b.Nodes().OfType<XElement>().Select(v => DeSerializeCard(v)).ToArray());
            return erg;
        }

        public XElement SerializeStack(Stack stack)
        {
            var root = new XElement(XName.Get("Stack"));

            var b = new XElement(XName.Get("Cards"));

            for (int i = 0; i < stack.Count; i++)
            {
                var v = SerializeCard(stack[i]);
                b.Add(v);
            }
            root.Add(b);

            return root;
        }

        public Card.UncoverdSecrets DeSerializeUncoverdSecrets(XElement root)
        {
            var erg = new Card.UncoverdSecrets();

            var b = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "B"));
            var index = int.Parse(root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "PlayerIndex").Value);

            erg.B = b.Nodes().OfType<XElement>().Select(v => int.Parse(v.Value)).ToArray();
            erg.PlayerIndex = index;
            return erg;
        }

        public XElement SerializeProveUncoverdSecrets(Card.UncoverdSecrets prove)
        {
            var root = new XElement(XName.Get("UncoverdSecrets"));

            var index = new XElement(XName.Get("PlayerIndex"), prove.PlayerIndex.ToString());
            var b = new XElement(XName.Get("B"));

            for (int i = 0; i < prove.B.Length; i++)
            {
                var v = new XElement(XName.Get("b"), prove.B[i]);
                b.Add(v);
            }
            root.Add(index, b);

            return root;
        }

        public Card.ProveCreatedFromPhase3Output DeSerializeProveCreatedFromPhase3Output(XElement root)
        {
            var erg = new Card.ProveCreatedFromPhase3Output();

            var x = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "Secrets"));

            erg.Secrets = x.Nodes().OfType<XElement>().Select(v => DeSerializeCardSeret(v.FirstNode as XElement)).ToArray();
            return erg;
        }

        public XElement SerializeProveCreatedFromPhase3Output(Card.ProveCreatedFromPhase3Output prove)
        {
            var root = new XElement(XName.Get("ProveCreatedFromPhase3Output"));

            var x = new XElement(XName.Get("Secrets"));

            for (int i = 0; i < prove.Secrets.Length; i++)
            {
                var v = new XElement(XName.Get("Secret"), SerializeCardSecret(prove.Secrets[i]));
                x.Add(v);
            }
            root.Add(x);

            return root;
        }

        public Card.ProveCreatedFromPhase2Output DeSerializeProveCreatedFromPhase2Output(XElement root)
        {
            var erg = new Card.ProveCreatedFromPhase2Output();

            var x = (root.Nodes().OfType<XElement>().Single(y => y.Name.LocalName == "X"));

            erg.X = x.Nodes().OfType<XElement>().Select(v => bool.Parse(v.Value)).ToArray();
            return erg;
        }

        public XElement SerializeProveCreatedFromPhase2Output(Card.ProveCreatedFromPhase2Output prove)
        {
            var root = new XElement(XName.Get("ProveCreatedFromPhase2Output"));

            var x = new XElement(XName.Get("X"));

            for (int i = 0; i < prove.X.Length; i++)
            {
                var v = new XElement(XName.Get("Card"), prove.X[i].ToString());
                x.Add(v);
            }
            root.Add(x);

            return root;
        }

        public Card.ProveCreatedFromPhase1Output DeSerializeProveCreatedFromPhase1Output(XElement root)
        {
            var erg = new Card.ProveCreatedFromPhase1Output();

            var c = DeSerializeCard(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "Card").FirstNode as XElement);
            var cl = (root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "Cl"));

            erg.C = c;
            erg.Cl = cl.Nodes().OfType<XElement>().Select(x => DeSerializeCard(x.FirstNode as XElement)).ToArray();
            return erg;
        }

        public XElement SerializeProveCreatedFromPhase1Output(Card.ProveCreatedFromPhase1Output prove)
        {
            var root = new XElement(XName.Get("ProveCreatedFromPhase1Output"));

            var c = new XElement(XName.Get("Card"), SerializeCard(prove.C));
            var cl = new XElement(XName.Get("Cl"));

            for (int i = 0; i < prove.Cl.Length; i++)
            {
                var v = new XElement(XName.Get("Card"), SerializeCard(prove.Cl[i]));
                cl.Add(v);
            }
            root.Add(c, cl);

            return root;
        }

        public Card DeSerializeCard(XElement root)
        {
            var k = int.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "k").Value);
            var w = int.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "w").Value);
            if (this.W != w || this.K != k)
                throw new ArgumentException("PlayerCount or MaxNumber is Wrong");

            var z = (XElement)root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "z");

            var c = new Card(this);

            var enumerator = z.Nodes().GetEnumerator();
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    enumerator.MoveNext();
                    var v = (XElement)enumerator.Current;

                    c.values[i, j] = BigInteger.Parse(v.Value);
                }
            return c;
        }

        public XElement SerializeCard(Card c)
        {
            var root = new XElement(XName.Get("Card"));

            var k = new XElement(XName.Get("k"), c.k);
            var w = new XElement(XName.Get("w"), c.w);

            var values = new XElement(XName.Get("z"));
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    var v = new XElement(XName.Get("Value"));
                    v.SetValue(c.values[i, j].ToString());
                    values.Add(v);
                }
            root.Add(k, w, values);

            return root;
        }

        internal CardSecret DeSerializeCardSeret(XElement root)
        {
            var k = int.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "k").Value);
            var w = int.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "w").Value);
            if (this.W != w || this.K != k)
                throw new ArgumentException("PlayerCount or MaxNumber is Wrong");

            var r = (XElement)root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "r");
            var b = (XElement)root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "b");

            var c = new CardSecret(this);

            var enumeratorr = r.Nodes().GetEnumerator();
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    enumeratorr.MoveNext();
                    var v = (XElement)enumeratorr.Current;

                    c.r[i, j] = BigInteger.Parse(v.Value);
                }

            var enumeratorb = b.Nodes().GetEnumerator();
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    enumeratorb.MoveNext();
                    var v = (XElement)enumeratorb.Current;

                    c.b[i, j] = int.Parse(v.Value);
                }

            return c;
        }

        internal XElement SerializeCardSecret(CardSecret c)
        {
            var root = new XElement(XName.Get("CardSecret"));

            var k = new XElement(XName.Get("k"), c.k);
            var w = new XElement(XName.Get("w"), c.w);

            var r = new XElement(XName.Get("r"));
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    var v = new XElement(XName.Get("Value"));
                    v.SetValue(c.r[i, j].ToString());
                    r.Add(v);
                }

            var b = new XElement(XName.Get("b"));
            for (int i = 0; i < c.k; i++)
                for (int j = 0; j < c.w; j++)
                {
                    var v = new XElement(XName.Get("Value"));
                    v.SetValue(c.b[i, j].ToString());
                    b.Add(v);
                }

            root.Add(k, w, r, b);

            return root;
        }

        public static PublicGameKey DeSerializeKey(XElement root)
        {
            var m = BigInteger.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "m").Value);
            var y = BigInteger.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "y").Value);
            var n = int.Parse(root.Nodes().OfType<XElement>().Single(x => x.Name.LocalName == "n").Value);

            var key = new PublicGameKey(y, m, n);
            return key;
        }

        public static XElement SerializeKey(PublicGameKey key)
        {
            var root = new XElement(XName.Get("Key"));
            var m = new XElement(XName.Get("m"), key.M);
            var y = new XElement(XName.Get("y"), key.Y);
            var n = new XElement(XName.Get("n"), key.Playernumber);

            root.Add(m, y, n);
            return root;
        }

        #endregion Serialisation
    }
}
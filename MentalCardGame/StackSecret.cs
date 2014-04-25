using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalCardGame
{
    internal class StackSecret : List<Tuple<int, CardSecret>>
    {
        internal static StackSecret Combine(StackSecret s1, StackSecret s2)
        {
            if (s1.Count != s2.Count)
                throw new ArgumentException("Both secrets must have the same size. (" + s1.Count + " != " + s2.Count + ")");

            var erg = new StackSecret();

            for (int i = 0; i < s1.Count; i++)
            {
                erg.Add(Tuple.Create(s1[s2[i].Item1].Item1, CardSecret.Combine(s1[s2[i].Item1].Item2, s2[i].Item2)));
            }

            return erg;
        }
    }
}
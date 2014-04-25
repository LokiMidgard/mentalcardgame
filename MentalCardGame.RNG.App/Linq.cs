using System.Collections.Generic;

namespace MentalCardGame.RNG
{
    internal static class CollectionsAddOn
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> col, T element)
        {
            foreach (var t in col)
                yield return t;
            yield return element;
        }
    }
}
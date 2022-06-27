using System;
using System.Collections.Generic;

namespace ImageToPaletteGenerator
{
    public static class ListUtility
    {
        public static void Shuffle<T>(this IList<T> ts)
        {
            Random rng = new Random();
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = rng.Next(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
        }
    }
}
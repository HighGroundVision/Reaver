using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver
{
    public static class ListDeconstructionExtensions
    {
        public static void Deconstruct<T>(this IList<T> list, out T? first)
        {
            first = list is null ? default(T) : list.Count > 0 ? list[0] : default(T);
        }

        public static void Deconstruct<T>(this IList<T> list, out T? first, out T? second)
        {
            first = list.Count > 0 ? list[0] : default(T);
            second = list.Count > 1 ? list[1] : default(T);
        }

        public static void Deconstruct<T>(this IList<T> list, out T? first, out T? second, out T? thrid)
        {
            first = list.Count > 0 ? list[0] : default(T);
            second = list.Count > 1 ? list[1] : default(T);
            thrid = list.Count > 2 ? list[2] : default(T);
        }
    }
}

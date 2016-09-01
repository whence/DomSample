using System;
using System.Collections.Generic;

namespace DomSample.Utils
{
    public static class CollectionExtensions
    {
        public static void RemoveRange<T>(this List<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Remove(item);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var random = new Random(Maths.RandomInt32());

            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(list.Count - i) + i;
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static void Shuffle<T>(this IList<T> list, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Shuffle(list);
            }
        }

        public static TKeyOut[] ToKeyArray<TKeyIn, TKeyOut, TValue>(this IDictionary<TKeyIn, TValue> dict, Converter<TKeyIn, TKeyOut> converter)
        {
            var array = new TKeyOut[dict.Count];
            int index = 0;
            foreach (KeyValuePair<TKeyIn, TValue> pair in dict)
            {
                array[index++] = converter(pair.Key);
            }
            return array;
        }
    }
}

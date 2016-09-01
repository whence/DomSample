using System;
using System.Collections;
using System.Collections.Generic;

namespace DomSample.Utils
{
    /// <summary>
    /// Provider helper functions for <see cref="IEnumerable{T}"/> classes.
    /// </summary>
    public static class EnumerableHelper
    {
        /// <summary>
        /// Determines if the given object is part of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><c>source</c> is null.</exception>
        public static bool Contains<T>(IEnumerable<T> source, T obj)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            ICollection<T> collection = source as ICollection<T>;
            if (collection != null)
                return collection.Contains(obj);

            foreach (T item in source)
            {
                if (item.Equals(obj))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Get the item at the given index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><c>source</c> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is out of range.</exception>
        public static T ElementAt<T>(IEnumerable<T> source, int index)
        {
            T current;
            if (source == null)
                throw new ArgumentNullException("source");

            IList<T> list = source as IList<T>;
            if (list != null)
                return list[index];

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                        throw new ArgumentOutOfRangeException("index");

                    if (index == 0)
                    {
                        current = enumerator.Current;
                        break;
                    }
                    index--;
                }
            }

            return current;
        }

        /// <summary>
        /// Get the number of items in the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><c>source</c> is null.</exception>
        public static int Count<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            ICollection<T> collection = source as ICollection<T>;
            if (collection != null)
                return collection.Count;

            int num = 0;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    num++;
            }
            return num;
        }

        /// <summary>
        /// Get if the given list is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(IEnumerable<T> list)
        {
            return Count(list) == 0;
        }

        /// <summary>
        /// Cast a <see cref="IEnumerable{T}"/> to a new <see cref="IEnumerable{T}"/> of the given type.
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<Out> Cast<Out>(IEnumerable source)
        {
            IEnumerable<Out> outs = source as IEnumerable<Out>;
            if (outs != null)
                return outs;

            List<Out> list = new List<Out>();
            foreach (object item in source)
                list.Add((Out) item);

            return list.ToArray();
        }

        /// <summary>
        /// Given a value, try get the first key.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the key
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the value
        /// </typeparam>
        /// <param name="enumerable">
        /// A list of <see cref="KeyValuePair{TK, TV}"/>.
        /// </param>
        /// <param name="value">
        /// The value object.
        /// </param>
        /// <param name="key">
        /// The first key which is mapped to the given <paramref name="value"/> object.
        /// </param>
        /// <returns>
        /// <returns>
        /// <c>true</c>:
        ///     If a key was found.
        /// <c>false</c>:
        ///     If the <paramref name="value"/> did not exist in the given <paramref name="enumerable"/>.
        /// </returns>
        public static bool TryGetKey<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TValue value, out TKey key)
        {
            foreach (KeyValuePair<TKey, TValue> valuePair in enumerable)
            {
                if (Equals(valuePair.Value, value))
                {
                    key = valuePair.Key;
                    return true;
                }
            }

            key = default(TKey);
            return false;
        }

        /// <summary>
        /// Given a key, try to get its mapped value object.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the key
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the value
        /// </typeparam>
        /// <param name="enumerable">
        /// A list of <see cref="KeyValuePair{TK, TV}"/>.
        /// </param>
        /// <param name="key">
        /// The key object.
        /// </param>
        /// <param name="value">
        /// The mapped value object.
        /// </param>
        /// <returns>
        /// <c>true</c>:
        ///     If the <paramref name="value"/> was returned successfully.
        /// <c>false</c>:
        ///     If the <paramref name="key"/> did not exist in the given <paramref name="enumerable"/>.
        /// </returns>
        public static bool TryGetValue<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey key, out TValue value)
        {
            IDictionary<TKey, TValue> dictionary = enumerable as IDictionary<TKey, TValue>;
            if (dictionary != null)
            {
                dictionary.TryGetValue(key, out value);
                return true;
            }

            foreach (KeyValuePair<TKey, TValue> valuePair in enumerable)
            {
                if (Equals(valuePair.Key, key))
                {
                    value = valuePair.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }
    }
}

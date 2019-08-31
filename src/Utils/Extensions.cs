using System;
using System.Collections.Generic;
using System.Linq;

namespace Rodkulman.Telegram
{
    public static class Extensions
    {
        private static readonly Random rnd = new Random();

        /// <summary>
        /// Gets a random element from an array
        /// </summary>
        public static T GetRandomElement<T>(this T[] array)
        {
            if (array == null || !array.Any())
            {
                return default(T);
            }

            return array[rnd.Next(0, array.Length)];
        }

        /// <summary>
        /// Gets a random element from a enumerable
        /// </summary>
        public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || !enumerable.Any())
            {
                return default(T);
            }

            return enumerable.ElementAt(rnd.Next(0, enumerable.Count()));
        }
    }
}
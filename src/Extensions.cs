using System;
using System.Collections.Generic;
using System.Linq;

namespace Rodkulman.Telegram
{
    public static class Extensions
    {
        private static readonly Random rnd = new Random();
        public static T GetRandomElement<T>(this T[] array)
        {
            return array[rnd.Next(0, array.Length)];
        }

        public static T GetRandomElement<T>(this IEnumerable<T> array)
        {
            return array.ElementAt(rnd.Next(0, array.Count()));
        }
    }
}
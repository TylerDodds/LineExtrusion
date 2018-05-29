using System;
using System.Collections.Generic;

namespace BabyDinoHerd.Utility.Experimental
{
    [BabyDinoHerd.Experimental]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts a list of a certain type to an array of a different type.
        /// </summary>
        public static V[] ConvertTypeListToArray<U, V>(this IList<U> points, Func<U, V> createNewType)
        {
            V[] array = new V[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                array[i] = createNewType(points[i]);
            }
            return array;
        }

        /// <summary>
        /// Gets the index of the minimum value in a list, via <paramref name="comparer"/> comparison function, within an optional index range.
        /// </summary>
        /// <typeparam name="T">The type of values in the list.</typeparam>
        /// <param name="list">List of value.</param>
        /// <param name="comparer">Comparison function.</param>
        /// <param name="minIndexInclusive">Minimum index to search from, inclusive.</param>
        /// <param name="maxIndexExclusive">Maximum index to search to, exclusive.</param>
        public static int IndexOfMin<T>(this IList<T> list, Comparison<T> comparer, int minIndexInclusive = 0, int maxIndexExclusive = int.MaxValue)
        {
            maxIndexExclusive = Math.Min(maxIndexExclusive, list.Count);
            minIndexInclusive = Math.Max(0, minIndexInclusive);

            int indexOfMin = -1;

            if (minIndexInclusive < list.Count && minIndexInclusive < maxIndexExclusive)
            {
                indexOfMin = minIndexInclusive;
                T valueOfMin = list[indexOfMin];

                int testIndex = minIndexInclusive;
                while (testIndex < maxIndexExclusive)
                {
                    var currentValue = list[testIndex];
                    var comparison = comparer(currentValue, valueOfMin);
                    if(comparison < 0)
                    {
                        indexOfMin = testIndex;
                        valueOfMin = currentValue;
                    }

                    testIndex++;
                }
            }

            return indexOfMin;
        }
    }
}
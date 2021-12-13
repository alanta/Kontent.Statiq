using System;
using System.Collections.Generic;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extension methods enriching the LINQ syntax.
    /// </summary>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Returns distinct elements from a sequence by using an equality comparer based on specified properties.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the selector key.</typeparam>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="keySelector">Selector allowing to specify one or more properties to base the filtering on.</param>
        /// <returns><see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
        }
    }
}

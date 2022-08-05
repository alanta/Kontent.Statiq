using Kontent.Ai.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Linq;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extensions for working with typed content.
    /// </summary>
    public static class TypedContentExtensions
    {
        /// <summary>
        /// The key used in Statiq documents to store the Kontent item.
        /// </summary>
        [Obsolete("Please use KontentKeys.Item instead")]
        public const string KontentItemKey = KontentKeys.Item;
        
        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <typeparam name="TModel">The type of content to return.</typeparam>
        /// <returns>The strong typed content model.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called on a document that doesn't a Kontent content item.</exception>
        public static TModel AsKontent<TModel>(this IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), $"Expected a document of type {typeof(TModel).FullName}");
            }

            if (document.TryGetValue(KontentKeys.Item, out TModel contentItem))
            {
                return contentItem;
            }

            throw new InvalidOperationException($"This is not a Kontent document: {document.Source}");
        }

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <returns>The strong typed content model.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called on a document that doesn't a Kontent content item.</exception>
        public static ITaxonomyGroup AsKontentTaxonomy(this IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Expected a document but got <null>");
            }

            if (document.TryGetValue(KontentKeys.Taxonomy.Group, out ITaxonomyGroup contentItem))
            {
                return contentItem;
            }

            throw new InvalidOperationException($"This document does not contain a Kontent taxonomy group: {document.Source}");
        }

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <param name="key">The metadata key to fetch the term from.</param>
        /// <returns>The taxonomy term or <c>null</c>.</returns>
        public static ITaxonomyTerm? AsKontentTaxonomyTerm(this IDocument document, string key = KontentKeys.Taxonomy.Term)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Expected a document but got <null>");
            }

            if (document.TryGetValue(key, out ITaxonomyTerm term))
            {
                return term;
            }
            
            if (document.TryGetValue(KontentKeys.Taxonomy.Term, out ITaxonomyTermDetails termDetails))
            {
                return TaxonomyTerm.CreateFrom(termDetails);
            }

            return null;
        }

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <param name="key">The metadata key to get the terms from.</param>
        /// <returns>The taxonomy terms or an empty array.</returns>
        public static ITaxonomyTerm[] AsKontentTaxonomyTerms(this IDocument document, string key = KontentKeys.Taxonomy.Terms)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Expected a document but got <null>");
            }

            if (document.TryGetValue(key, out object? terms))
            {
                return terms switch
                {
                    ITaxonomyTerm[] items => items,
                    ITaxonomyTermDetails[] details => details.Select(TaxonomyTerm.CreateFrom).ToArray(),
                    _ => Array.Empty<ITaxonomyTerm>()
                };
            }

            return Array.Empty<ITaxonomyTerm>();
        }
    }
}

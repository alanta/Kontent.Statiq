using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;

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
        public const string KontentItemKey = "KONTENT";
        public const string KontentTaxonomyGroupKey = "KONTENT-TAXONOMY-GROUP";
        public const string KontentTaxonomyTermsKey = "KONTENT-TAXONOMY-TERMS";
        public const string KontentTaxonomyTermKey = "KONTENT-TAXONOMY-TERM";

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

            if (document.TryGetValue(KontentItemKey, out TModel contentItem))
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

            if (document.TryGetValue(KontentTaxonomyGroupKey, out ITaxonomyGroup contentItem))
            {
                return contentItem;
            }

            throw new InvalidOperationException($"This document does not contain a Kontent taxonomy group: {document.Source}");
        }

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <typeparam name="TModel">The type of content to return.</typeparam>
        /// <returns>The strong typed content model.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called on a document that doesn't a Kontent content item.</exception>
        public static ITaxonomyTerm? AsKontentTaxonomyTerm(this IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Expected a document but got <null>");
            }

            if (document.TryGetValue(KontentTaxonomyTermKey, out ITaxonomyTerm contentItem))
            {
                return contentItem;
            }

            return null;
        }

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <typeparam name="TModel">The type of content to return.</typeparam>
        /// <returns>The strong typed content model.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called on a document that doesn't a Kontent content item.</exception>
        public static ITaxonomyTerm[] AsKontentTaxonomyTerms(this IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Expected a document but got <null>");
            }

            if (document.TryGetValue(KontentTaxonomyTermsKey, out ITaxonomyTerm[] contentItem))
            {
                return contentItem;
            }

            return Array.Empty<ITaxonomyTerm>();
        }
    }
}

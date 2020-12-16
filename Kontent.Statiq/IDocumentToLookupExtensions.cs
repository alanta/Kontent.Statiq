using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extensions for creating lookups from document sequences.
    /// </summary>
    public static class IDocumentToLookupExtensions
    {
        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
        /// that contains a sequence of Kontent taxonomy terms.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<ITaxonomyTerm, IDocument> ToLookupManyByTaxonomy(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey)
        {
            documents.ThrowIfNull(nameof(documents));
            keyMetadataKey.ThrowIfNull(nameof(keyMetadataKey));

            return documents.ToLookupMany(keyMetadataKey, new TaxonomyTermComparer());
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
        /// that contains a sequence of Kontent taxonomy terms.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="getTaxonomy">A function to access the taxonomy terms of the content.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<ITaxonomyTerm, IDocument> ToLookupManyByTaxonomy<TPage>(
            this IEnumerable<IDocument> documents,
            Func<TPage,IEnumerable<ITaxonomyTerm>> getTaxonomy)
        {
            documents.ThrowIfNull(nameof(documents));
            getTaxonomy.ThrowIfNull(nameof(getTaxonomy));

            return documents.ToLookupMany( doc => getTaxonomy(doc.AsKontent<TPage>()), new TaxonomyTermComparer());
        }
    }
}
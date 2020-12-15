using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
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
        /// <typeparam name="TKey">The type of the key.</typeparam>
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
    }
}
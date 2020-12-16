using Kentico.Kontent.Delivery.Abstractions;
using System.Collections.Generic;

namespace Kontent.Statiq
{
    /// <summary>
    /// Comparer for Kontent taxonomy.
    /// <para>Use this with <see cref="global::Statiq.Common.IDocumentToLookupExtensions.ToLookupMany{TKey}(IEnumerable{Statiq.Common.IDocument}, string, IEqualityComparer{TKey})"/></para>
    /// </summary>
    public class TaxonomyTermComparer : IEqualityComparer<ITaxonomyTerm>
    {
        /// <inheritdoc />
        public bool Equals(ITaxonomyTerm? x, ITaxonomyTerm? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.Codename == y.Codename;
        }

        /// <inheritdoc />
        public int GetHashCode(ITaxonomyTerm obj)
        {
            return (obj.Codename != null ? obj.Codename.GetHashCode() : 0);
        }
    }
}

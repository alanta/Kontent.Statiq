using Kentico.Kontent.Delivery.Abstractions;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extension methods to configure the Kontent.Statiq Taxonomy module.
    /// </summary>
    public static class KontentTaxonomyModuleConfiguration
    {
        /// <summary>
        /// Add query parameters.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="queryParameters"></param>
        /// <returns>The module.</returns>
        public static KontentTaxonomy WithQuery(this KontentTaxonomy module, params IQueryParameter[] queryParameters)
        {
            module.QueryParameters.AddRange(queryParameters);
            return module;
        }
    }
}
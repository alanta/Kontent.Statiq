using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Urls.Delivery.QueryParameters;
using System;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extension methods to configure the Kontent.Statiq module.
    /// </summary>
    public static class KontentModuleConfiguration
    {
        /// <summary>
        /// Get a string to use as content in the Statiq document.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="field">A func that returns a string to be used as content.</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithContent<TContentModel>(this Kontent<TContentModel> module, Func<TContentModel, string> field) where TContentModel : class
        {
            module.GetContent = field;
            return module;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        [Obsolete("Please use WithQuery to add query parameters instead.")]
        public static Kontent<TContentModel> OrderBy<TContentModel>(this Kontent<TContentModel> module, string field, SortOrder sortOrder) where TContentModel : class
        {
            if (!field.StartsWith("elements") && !field.StartsWith("system"))
            {
                field = "elements." + field;
            }

            module.QueryParameters.Add(new OrderParameter(field, sortOrder));
            return module;
        }

        /// <summary>
        /// Skip the specified number of items. Use this with Limit to page the results.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        [Obsolete("Please use WithQuery to add query parameters instead.")]
        public static Kontent<TContentModel> Skip<TContentModel>(this Kontent<TContentModel> module, int skip) where TContentModel : class
        {
            module.QueryParameters.Add(new SkipParameter(skip));
            return module;
        }

        /// <summary>
        /// Set the maximum number of items to return. Use this with Skip to page the results.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        [Obsolete("Please use WithQuery to add query parameters instead.")]
        public static Kontent<TContentModel> Limit<TContentModel>(this Kontent<TContentModel> module, int limit) where TContentModel : class
        {
            module.QueryParameters.Add(new LimitParameter(limit));
            return module;
        }

        /// <summary>
        /// Add Kontent query parameters.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="queryParameters"></param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithQuery<TContentModel>(this Kontent<TContentModel> module, params IQueryParameter[] queryParameters) where TContentModel : class
        {
            module.QueryParameters.AddRange(queryParameters);
            return module;
        }
    }
}
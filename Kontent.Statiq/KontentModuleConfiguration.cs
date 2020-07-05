using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Urls.QueryParameters;
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
        public static Kontent<TContentModel> OrderBy<TContentModel>(this Kontent<TContentModel> module, string field, SortOrder sortOrder) where TContentModel : class
        {
            if (!field.StartsWith("elements") && !field.StartsWith("system"))
            {
                field = "elements." + field;
            }

            module.QueryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.Urls.QueryParameters.SortOrder)sortOrder));
            return module;
        }

        /// <summary>
        /// Skip the specified number of items. Use this with Limit to page the results.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
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

        /// <summary>
        /// Set the content provider instance to use. 
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeProvider">The type provider to use.</param>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithTypeProvider<TContentModel>(this Kontent<TContentModel> module, ITypeProvider typeProvider) where TContentModel : class
        {
            module.ConfigureClientActions.Add(builder => builder.WithTypeProvider(typeProvider));
            return module;
        }

        /// <summary>
        /// Set the content provider to use. The provider must have a default constructor.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <typeparam name="TTypeProvider">The type of the type provider to use.</typeparam>
        /// <typeparam name="TContentModel">The content model type.</typeparam>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithTypeProvider<TTypeProvider, TContentModel>(this Kontent<TContentModel> module)
            where TTypeProvider : ITypeProvider, new()
            where TContentModel : class
        {
            module.ConfigureClientActions.Add(builder => builder.WithTypeProvider(new TTypeProvider()));
            return module;
        }

        /// <summary>
        /// Add an inline item resolver for the specified content type.
        /// </summary>
        /// <typeparam name="TContentModel">The content model.</typeparam>
        /// <typeparam name="TInlineContentType">The type of the inline content item.</typeparam>
        /// <param name="module">The module.</param>
        /// <param name="resolver">The content resolver instance.</param>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithInlineItemResolver<TContentModel, TInlineContentType>(this Kontent<TContentModel> module, IInlineContentItemsResolver<TInlineContentType> resolver)
            where TContentModel : class
        {
            module.ConfigureClientActions.Add(builder => builder.WithInlineContentItemsResolver(resolver));
            return module;
        }

        /// <summary>
        /// Add an inline item resolver for the specified content type.
        /// </summary>
        /// <typeparam name="TContentType">The content type for which this resolver is used.</typeparam>
        /// <typeparam name="TResolver">The type of the content resolver to use. Must have a default constructor.</typeparam>
        /// <typeparam name="TContentModel">The content model.</typeparam>
        /// <param name="module">The module.</param>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithInlineItemResolver<TContentModel, TContentType, TResolver>(this Kontent<TContentModel> module)
            where TContentModel : class
            where TResolver : IInlineContentItemsResolver<TContentType>, new()
        {
            module.ConfigureClientActions.Add(builder => builder.WithInlineContentItemsResolver(new TResolver()));
            return module;
        }

        /// <summary>
        /// Use the specified production API key. This disables the preview API key if that was configured earlier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="apiKey">The API key. You can find this key in the Kontent portal.</param>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithProductionApiKey<TContentModel>(this Kontent<TContentModel> module, string apiKey)
            where TContentModel : class
        {
            module.PreviewApiKey = string.Empty;
            module.ProductionApiKey = apiKey;
            return module;
        }

        /// <summary>
        /// Use the specified preview API key. This disables a production API key if that was configured earlier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="apiKey">The API key. You can find this key in the Kontent portal.</param>
        /// <returns>The module.</returns>
        public static Kontent<TContentModel> WithPreviewApiKey<TContentModel>(this Kontent<TContentModel> module, string apiKey)
            where TContentModel : class
        {
            module.ProductionApiKey = string.Empty;
            module.PreviewApiKey = apiKey;
            return module;
        }
    }
}
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Abstractions.InlineContentItems;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extension methods to configure the Kontent.Statiq module.
    /// </summary>
    public static class KontentModuleConfiguration
    {
        /// <summary>
        /// Sets the content type to retrieve.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="contentType">Code name of the content type to retrieve. This for untyped content only.</param>
        /// <returns></returns>
        public static Kontent WithContentType(this Kontent module, string contentType)
        {
            module.QueryParameters.Add(new EqualsFilter("system.type", contentType));
            return module;
        }

        /// <summary>
        /// Sets the main content field. This is mostly useful for untyped content.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="field">Field code name, this is case sensitive</param>
        /// <returns>The module.</returns>
        public static Kontent WithContentField(this Kontent module, string field)
        {
            module.ContentField = field;
            return module;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns>The module.</returns>
        public static Kontent OrderBy(this Kontent module, string field, SortOrder sortOrder)
        {
            module.QueryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.Abstractions.SortOrder)sortOrder));
            return module;
        }

        /// <summary>
        /// Skip the specified number of items. Use this with Limit to page the results.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <returns>The module.</returns>
        public static Kontent Skip(this Kontent module, int skip)
        {
            module.QueryParameters.Add(new SkipParameter(skip));
            return module;
        }

        /// <summary>
        /// Set the maximum number of items to return. Use this with Skip to page the results.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The module.</returns>
        public static Kontent Limit(this Kontent module, int limit)
        {
            module.QueryParameters.Add(new LimitParameter(limit));
            return module;
        }

        /// <summary>
        /// Add Kontent query parameters.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="queryParameters"></param>
        /// <returns>The module.</returns>
        public static Kontent WithQuery(this Kontent module, params IQueryParameter[] queryParameters)
        {
            module.QueryParameters.AddRange(queryParameters);
            return module;
        }
        
        /// <summary>
        /// Set the content provider instance to use. 
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeProvider">The type provider to use.</param>
        /// <returns>The module.</returns>
        public static Kontent WithTypeProvider(this Kontent module, ITypeProvider typeProvider)
        {
            module.ConfigureClientActions.Add( builder => builder.WithTypeProvider( typeProvider ) );
            return module;
        }

        /// <summary>
        /// Set the content provider to use. The provider must have a default constructor.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <typeparam name="TTypeProvider">The type of the type provider to use.</typeparam>
        /// <returns>The module.</returns>
        public static Kontent WithTypeProvider<TTypeProvider>(this Kontent module) where TTypeProvider : ITypeProvider, new()
        {
            module.ConfigureClientActions.Add(builder => builder.WithTypeProvider(new TTypeProvider()));
            return module;
        }

        /// <summary>
        /// Add an inline item resolver for the specified content type.
        /// </summary>
        /// <typeparam name="TContentType">The content type for which this resolver is used.</typeparam>
        /// <param name="module">The module.</param>
        /// <param name="resolver">The content resolver instance.</param>
        /// <returns>The module.</returns>
        public static Kontent WithInlineItemResolver<TContentType>(this Kontent module, IInlineContentItemsResolver<TContentType> resolver )
        {
            module.ConfigureClientActions.Add(builder => builder.WithInlineContentItemsResolver( resolver ));
            return module;
        }

        /// <summary>
        /// Add an inline item resolver for the specified content type.
        /// </summary>
        /// <typeparam name="TContentType">The content type for which this resolver is used.</typeparam>
        /// <typeparam name="TResolver">The type of the content resolver to use. Must have a default constructor.</typeparam>
        /// <param name="module">The module.</param>
        /// <returns>The module.</returns>
        public static Kontent WithInlineItemResolver<TContentType,TResolver>(this Kontent module) where TResolver : IInlineContentItemsResolver<TContentType>, new()
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
        public static Kontent WithProductionApiKey(this Kontent module, string apiKey)
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
        public static Kontent WithPreviewApiKey(this Kontent module, string apiKey)
        {
            module.ProductionApiKey = string.Empty;
            module.PreviewApiKey = apiKey;
            return module;
        }
    }
}
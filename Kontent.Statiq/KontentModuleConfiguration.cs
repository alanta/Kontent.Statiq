using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Abstractions.InlineContentItems;

namespace Kontent.Statiq
{
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
        /// Sets the main content field. This for untyped content only and is case sensitive.
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns></returns>
        public static Kontent WithContentField(this Kontent module, string field)
        {
            module.ContentField = field;
            return module;
        }

        public static Kontent WithUrlField(this Kontent module, string field)
        {
            module.UrlField = field;
            return module;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns></returns>
        public static Kontent OrderBy(this Kontent module, string field, SortOrder sortOrder)
        {
            module.QueryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.Abstractions.SortOrder)sortOrder));
            return module;
        }

        public static Kontent WithTypeProvider(this Kontent module, ITypeProvider typeProvider)
        {
            module.ConfigureClientActions.Add( builder => builder.WithTypeProvider( typeProvider ) );
            return module;
        }

        public static Kontent WithTypeProvider<TTypeProvider>(this Kontent module) where TTypeProvider : ITypeProvider, new()
        {
            module.ConfigureClientActions.Add(builder => builder.WithTypeProvider(new TTypeProvider()));
            return module;
        }

        public static Kontent WithInlineItemResolver<TContentType>(this Kontent module, IInlineContentItemsResolver<TContentType> resolver )
        {
            module.ConfigureClientActions.Add(builder => builder.WithInlineContentItemsResolver( resolver ));
            return module;
        }

        public static Kontent WithInlineItemResolver<TContentType,TResolver>(this Kontent module) where TResolver : IInlineContentItemsResolver<TContentType>, new()
        {
            module.ConfigureClientActions.Add(builder => builder.WithInlineContentItemsResolver(new TResolver()));
            return module;
        }

        public static Kontent WithProductionApiKey(this Kontent module, string apiKey)
        {
            module.PreviewApiKey = string.Empty;
            module.ProductionApiKey = apiKey;
            return module;
        }

        public static Kontent WithPreviewApiKey(this Kontent module, string apiKey)
        {
            module.ProductionApiKey = string.Empty;
            module.PreviewApiKey = apiKey;
            return module;
        }
    }
}
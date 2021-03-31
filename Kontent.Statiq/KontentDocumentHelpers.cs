using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kontent.Statiq
{
    /// <summary>
    /// Helpers to construct Statiq IDocument instances from Kontent items.
    /// </summary>
    internal static class KontentDocumentHelpers
    {
        internal static IDocument CreateDocument<TContentModel>(IExecutionContext context, TContentModel item, Func<TContentModel, string>? getContent) where TContentModel : class
        {
            var props = typeof(TContentModel).GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                                            BindingFlags.GetProperty | BindingFlags.Public);

            var content = getContent?.Invoke(item) ?? "";

            return CreateDocumentInternal(context, item, props, content);

        }

        internal static IDocument CreateDocument(IExecutionContext context, object item, Func<object, string>? getContent)
        {
            var props = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                                     BindingFlags.GetProperty | BindingFlags.Public);

            var content = getContent?.Invoke(item) ?? "";

            return CreateDocumentInternal(context, item, props, content);
        }

        private static IDocument CreateDocumentInternal(IExecutionContext context, object item, PropertyInfo[] props, string content )
        {
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(KontentKeys.Item, item),
            };
            
            MapSystemMetadata(item, props, metadata);

            return context.CreateDocument(metadata, content, "text/html");
        }

        /// <summary>
        /// Map Kontent system properties directly into document metadata.
        /// </summary>
        /// <param name="item">The Kontent item.</param>
        /// <param name="props"></param>
        /// <param name="metadata"></param>
        private static void MapSystemMetadata(object item, PropertyInfo[] props, List<KeyValuePair<string, object>> metadata)
        {
            if (props.FirstOrDefault(prop => typeof(IContentItemSystemAttributes).IsAssignableFrom(prop.PropertyType))
                ?.GetValue(item) is IContentItemSystemAttributes systemProp)
            {
                metadata.AddRange(new[]
                {
                    new KeyValuePair<string, object>(KontentKeys.System.Name, systemProp.Name),
                    new KeyValuePair<string, object>(KontentKeys.System.CodeName, systemProp.Codename),
                    new KeyValuePair<string, object>(KontentKeys.System.Language, systemProp.Language),
                    new KeyValuePair<string, object>(KontentKeys.System.Id, systemProp.Id),
                    new KeyValuePair<string, object>(KontentKeys.System.Type, systemProp.Type),
                    new KeyValuePair<string, object>(KontentKeys.System.LastModified, systemProp.LastModified),
                });
            }
        }
    }
}
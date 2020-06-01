using Kentico.Kontent.Delivery.Abstractions;
using Statiq.Common;
using System;

namespace Kontent.Statiq
{
    public static class TypedContentExtensions
    {
        public const string KontentItemKey = "KONTENT";

        public static TModel AsKontent<TModel>(this IDocument document)
        {
            if (document.TryGetValue(KontentItemKey, out ContentItem contentItem))
            {
                return contentItem.CastTo<TModel>();
            }

            throw new InvalidOperationException($"This is not a Kontent document: {document.Source}");
        }
    }
}

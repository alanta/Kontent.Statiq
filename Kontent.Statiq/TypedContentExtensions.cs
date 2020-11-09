using Statiq.Common;
using System;

namespace Kontent.Statiq
{
    /// <summary>
    /// Extensions for working with typed content.
    /// </summary>
    public static class TypedContentExtensions
    {
        /// <summary>
        /// The key used in Statiq documents to store the Kontent item.
        /// </summary>
        public const string KontentItemKey = "KONTENT";

        /// <summary>
        /// Return the strong typed model for a Statiq document.
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <typeparam name="TModel">The type of content to return.</typeparam>
        /// <returns>The strong typed content model.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called on a document that doesn't a Kontent content item.</exception>
        public static TModel AsKontent<TModel>(this IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), $"Expected a document of type {typeof(TModel).FullName}");
            }

            if (document.TryGetValue(KontentItemKey, out TModel contentItem))
            {
                return contentItem;
            }

            throw new InvalidOperationException($"This is not a Kontent document: {document.Source}");
        }
    }
}

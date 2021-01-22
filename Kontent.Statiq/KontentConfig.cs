using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontent.Statiq
{
    /// <summary>
    /// Kontent specific Config expressions
    /// </summary>
    public static class KontentConfig
    {
        /// <summary>
        /// Map related content into a collection of Statiq documents.
        /// </summary>
        /// <typeparam name="TContentType">The content type.</typeparam>
        /// <param name="getChildren">A function that returns a set of Kontent items.</param>
        /// <returns>A config object.</returns>
        public static Config<IEnumerable<IDocument>> GetChildren<TContentType>(Func<TContentType, IEnumerable<object>> getChildren)
        {
            if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

            return Config.FromDocument<IEnumerable<IDocument>>( (doc, ctx) =>
            {
                var list = new List<IDocument>();
                var parent = doc.AsKontent<TContentType>();
                
                if (parent != null)
                {
                    var children = getChildren(parent)?.ToArray() ?? Array.Empty<object>();
                    foreach (var item in children)
                    {
                        list.Add(KontentDocumentHelpers.CreateDocument(ctx, item, null));
                    }
                }

                return list;
            });
        }

        /// <summary>
        /// Map a value from a Kontent item.
        /// </summary>
        /// <typeparam name="TContentType">The Kontent model type.</typeparam>
        /// <typeparam name="TValue">The return value.</typeparam>
        /// <param name="getValue">A function that retrieves the value from the content.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> Get<TContentType, TValue>(Func<TContentType, TValue> getValue)
        {
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));

            return Config.FromDocument((doc, ctx) => getValue(doc.AsKontent<TContentType>()));
        }

        /// <summary>
        /// Map a document from a Kontent item.
        /// </summary>
        /// <typeparam name="TContentType">The Kontent model type.</typeparam>
        /// <returns>A config object.</returns>
        public static Config<TContentType> As<TContentType>()
        {
            return Config.FromDocument((doc, ctx) => doc.AsKontent<TContentType>());
        }
    }
}
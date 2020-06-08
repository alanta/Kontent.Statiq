using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.ImageTransformation;
using Statiq.Common;
using System.Collections.Generic;
using System.Linq;

namespace Kontent.Statiq.Html
{
    /// <summary>
    /// Helper methods to aid with Razor templates
    /// </summary>
    public static class HtmlHelpers
    {
        /// <summary>
        /// Build an image url from an <see cref="Asset"/>.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>An <see cref="ImageUrlBuilder"/> for the asset.</returns>
        public static ImageUrlBuilder ImageUrl(this Asset asset)
        {
            return new ImageUrlBuilder(asset.Url);
        }

        /// <summary>
        /// Get the first asset url from a document using the code name. This is intended for untyped content.
        /// </summary>
        /// <param name="document">The Statiq document.</param>
        /// <param name="codename">The codename of the field that holds the asset.</param>
        /// <returns>The asset url or an empty string if not available.</returns>
        public static string GetFirstAssetUrl(this IDocument document, string codename)
        {
            if (document == null)
            {
                return string.Empty;
            }

            var assets = document.Get<IEnumerable<Asset>>(codename);
            return assets?.FirstOrDefault()?.Url ?? string.Empty;
        }
    }
}

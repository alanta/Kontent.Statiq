using Kontent.Statiq.Models;
using Statiq.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kontent.Statiq.Html
{
    /// <summary>
    /// Helper methods to aid with Razor templates
    /// </summary>
    public static class HtmlHelpers
    {
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

            var assets = document.Get<List<Asset>>(codename);
            return assets != null && assets.Any() ? assets[0].Url : string.Empty;
        }

        /// <summary>
        /// Get the first asset url from a document for assets that are stored locally using the code name. This is intended for untyped content.
        /// </summary>
        /// <param name="document">The Statiq document.</param>
        /// <param name="codename">The codename of the field that holds the asset.</param>
        /// <param name="folderPath">The folder where local assets are stored.</param>
        /// <returns>The asset url or an empty string if not available.</returns>
        public static string GetFirstAssetLocalUrl(this IDocument document, string codename, string folderPath = "")
        {
            var assetUrl = GetFirstAssetUrl(document, codename);
            if (string.IsNullOrEmpty(assetUrl))
            {
                return string.Empty;
            }

            return Path.Combine("/", folderPath, KontentAssetHelper.GetAssetFileName(assetUrl)).Replace(@"\", "/");
        }
    }
}

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
        public static string GetFirstAssetUrl(this IDocument document, string codename)
        {
            if (document == null)
            {
                return string.Empty;
            }

            var assets = document.Get<List<Asset>>(codename);
            return assets != null && assets.Any() ? assets[0].Url : string.Empty;
        }

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

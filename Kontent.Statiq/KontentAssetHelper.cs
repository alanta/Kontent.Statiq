using Kontent.Statiq.Models;
using Statiq.Common;
using System.Collections.Generic;
using System.Linq;

namespace Kontent.Statiq
{
    public static class KontentAssetHelper
    {
        public static string[] GetAssetUris(IDocument doc)
        {
            var assetUrls = new List<string>();

            var assets = doc.Where(x => x.Value is List<Asset>).ToList();
            if (assets.Any())
            {
                foreach (var metaAsset in assets)
                {
                    var asset = (List<Asset>)metaAsset.Value;

                    foreach (var image in asset)
                    {
                        assetUrls.Add(image.Url);
                    }
                }
            }

            return assetUrls.ToArray();
        }

        public static string GetAssetFileName(IDocument doc)
        {
            var fileUrl = doc.Get<string>("SourceUri");
            return GetAssetFileName(fileUrl);
        }

        public static string GetAssetFileName(string fileUrl)
        {
            var splitPath = fileUrl.Split('/');
            return $"{splitPath[splitPath.Length - 2]}/{splitPath[splitPath.Length - 1]}";
        }
    }
}
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.ImageTransformation;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Kontent.Statiq
{
    /// <summary>
    /// Helpers to handle assets in untyped content.
    /// </summary>
    public static class KontentAssetHelper
    {
        private static MD5 _md5 = MD5.Create();

        /// <summary>
        /// Get all the assets uris in a Statiq document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>All the urls for assets in the document.</returns>
        public static string[] GetAssetUris(IDocument doc)
        {
            var assetUrls = new List<string>();

            var assets = doc.Select(x => x.Value).OfType<IEnumerable<Asset>>().ToList();
            if (assets.Any())
            {
                foreach (var asset in assets)
                {

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

        public static KontentAssetDownload[] GetKontentAssetDownloads(this IDocument doc)
        {
            return doc.Get<KontentAssetDownload[]>(KontentImageProcessor.KontentAssetDownloadKey) ?? Array.Empty<KontentAssetDownload>();
        }

        /// <summary>
        /// Given a url, determine what the local path is for the file.
        /// </summary>
        /// <param name="url">The file url.</param>
        /// <param name="localBasePath">The base path.</param>
        /// <returns>The local path.</returns>
        /// <exception cref="UriFormatException">Thrown when the url is in an invalid format.</exception>
        public static NormalizedPath GetLocalFileName(string url, NormalizedPath localBasePath)
        {
            var uri = new Uri(url);
            var fileName = ReplaceInvalidChars(Path.GetFileName(uri.LocalPath));

            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                
                // sort the query to ensure the same image gets the same URL
                var sortedQuery =
                    string.Join("&", query.AllKeys
                        .Select(key => (key, value: query[key]))
                        .OrderBy(kv => kv.key)
                        .Select(kv => $"{kv.key}={kv.value}"));
                
                // hash the query - no need to disclose what transforms were done on the image
                var hash = string.Join("", _md5
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(sortedQuery))
                    .Select(b => b.ToString("x2")));

                fileName = $"{hash}-{fileName}";
            }

            return System.IO.Path.Combine(localBasePath.FullPath, fileName).Replace("\\", "/");
        }

        private static string ReplaceInvalidChars(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filename));

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

    }
}
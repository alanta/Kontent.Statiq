using Statiq.Common;
using System;
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
        private static readonly MD5 Md5 = MD5.Create();

        /// <summary>
        /// Get the image downloads for the document. These are added by the <see cref="KontentImageProcessor"/>.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>An array of <see cref="KontentImageDownload"/> objects listing the image substitutions performed by <see cref="KontentImageProcessor"/>.</returns>
        public static KontentImageDownload[] GetKontentImageDownloads(this IDocument doc)
        {
            return doc.Get<KontentImageDownload[]>(KontentKeys.Images.Downloads) ?? Array.Empty<KontentImageDownload>();
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
                var hash = string.Join("", Md5
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(sortedQuery))
                    .Select(b => b.ToString("x2")));

                fileName = $"{hash}-{fileName}";
            }

            return Path.Combine(localBasePath.FullPath, fileName).Replace("\\", "/");
        }

        private static string ReplaceInvalidChars(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filename));

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

    }
}
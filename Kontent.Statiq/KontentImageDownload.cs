using Statiq.Common;

namespace Kontent.Statiq
{
    /// <summary>
    /// This is an image that is substituted by the <see cref="KontentImageProcessor"/>.
    /// </summary>
    public class KontentImageDownload
    {
        /// <summary>
        /// Create a new KontentImageDownload.
        /// </summary>
        /// <param name="originalUrl">The full original url.</param>
        /// <param name="localPath">The local path for the image.</param>
        public KontentImageDownload(string originalUrl, NormalizedPath localPath)
        {
            OriginalUrl = originalUrl;
            LocalPath = localPath;
        }

        /// <summary>
        /// The full original url for the image, including any query parameters.
        /// </summary>
        public string OriginalUrl { get; }

        /// <summary>
        /// The local path for the image.
        /// </summary>
        public NormalizedPath LocalPath { get; }
    }
}
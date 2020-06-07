using Statiq.Common;

namespace Kontent.Statiq
{
    public class KontentAssetDownload
    {
        public KontentAssetDownload(string originalUrl, NormalizedPath localPath)
        {
            OriginalUrl = originalUrl;
            LocalPath = localPath;
        }
        public string OriginalUrl { get; }
        public NormalizedPath LocalPath { get; }
    }
}
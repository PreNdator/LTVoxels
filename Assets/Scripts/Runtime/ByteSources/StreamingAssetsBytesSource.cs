using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    [CreateAssetMenu(menuName = "Game/Bytes Sources/StreamingAssets Bytes Source")]
    public sealed class StreamingAssetsBytesSource : AsyncBytesSource
    {
        [SerializeField, StreamingAssetsPath]
        private string _relativePath;

        public string RelativePath => _relativePath;

        public string FullPath => Application.streamingAssetsPath + _relativePath;

        public override async Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_relativePath))
                throw new InvalidOperationException("Relative path is null or empty.");

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return await LoadFromStreamingAssetsAndroidAsync(ct);
                default:
                    return await LoadFromFileSystemAsync(ct);
            }
        }

        private Task<byte[]> LoadFromFileSystemAsync(CancellationToken ct)
        {
            AsyncFileSystemBytesSource fileSystemBytesSource = new AsyncFileSystemBytesSource(FullPath);
            return fileSystemBytesSource.GetBytesAsync(ct);
        }

        private Task<byte[]> LoadFromStreamingAssetsAndroidAsync(CancellationToken ct)
        {
            WebRequestBytesSource webRequestBytesSource = new WebRequestBytesSource(FullPath);
            return webRequestBytesSource.GetBytesAsync(ct);
        }
    }
}
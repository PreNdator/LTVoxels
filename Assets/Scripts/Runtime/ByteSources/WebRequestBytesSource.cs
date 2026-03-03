using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace LedenevTV.Voxel.Serialization
{

    public sealed class WebRequestBytesSource : IAsyncBytesSource
    {
        private const int DefaultTimeoutSeconds = 30;

        public string Url { get; }

        public WebRequestBytesSource(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is null or empty.", nameof(url));

            Url = url;
        }

        public async Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(Url))
            {
                request.timeout = DefaultTimeoutSeconds;

                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest().WithCancellation(ct, cancelImmediately: true);

                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException($"Failed to load bytes '{Url}'. Result={request.result}, Code={request.responseCode}, Error={request.error}");

                return request.downloadHandler.data;
            }
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    [CreateAssetMenu(menuName = "Game/Bytes Sources/Web Request Bytes Source")]
    public sealed class WebRequestBytesSourceAsset : AsyncBytesSource
    {
        [SerializeField]
        private string _url;
        public string Url => _url;

        public override async Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            WebRequestBytesSource webRequestBytesSource = new WebRequestBytesSource(Url);
            return await webRequestBytesSource.GetBytesAsync(ct);
        }
    }
}
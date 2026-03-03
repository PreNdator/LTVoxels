using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    [CreateAssetMenu(menuName = "Game/Bytes Sources/Voxel Asset Bytes Source")]
    public sealed class VoxelAssetBytesSource : AsyncBytesSource
    {
        [SerializeField]
        private VoxelBytesAsset _asset;

        public VoxelBytesAsset Asset => _asset;

        public override Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            if (_asset == null)
                throw new InvalidOperationException("Asset is null.");

            return Task.FromResult(_asset.Data);
        }
    }
}
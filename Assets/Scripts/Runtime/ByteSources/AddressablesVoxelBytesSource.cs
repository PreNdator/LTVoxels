using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LedenevTV.Voxel.Serialization
{
    [CreateAssetMenu(menuName = "Game/Bytes Sources/Addressables Bytes Source")]
    public sealed class AddressablesVoxelBytesSource : AsyncBytesSource
    {
        [SerializeField]
        private AssetReferenceT<VoxelBytesAsset> _assetReference;
        public AssetReferenceT<VoxelBytesAsset> AssetReference => _assetReference;

        public override async Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            if (_assetReference == null)
                throw new InvalidOperationException("Addressable reference is null.");

            if (!_assetReference.RuntimeKeyIsValid())
                throw new InvalidOperationException($"Addressable runtime key is invalid. Reference: {_assetReference}");

            AsyncOperationHandle<VoxelBytesAsset> handle = _assetReference.LoadAssetAsync();

            try
            {
                VoxelBytesAsset asset = await handle.ToUniTask(cancellationToken: ct);

                if (asset == null)
                    throw new InvalidDataException("Loaded addressable TextAsset is null.");

                return asset.Data;
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }
}
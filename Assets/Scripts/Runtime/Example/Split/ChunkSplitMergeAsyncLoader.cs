using Cysharp.Threading.Tasks;
using LedenevTV.Voxel;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(AsyncChunkLoader), typeof(ChunkSplitMerge), typeof(MeshRenderer))]
    public sealed class ChunkSplitMergeAsyncLoader : MonoBehaviour
    {
        private AsyncChunkLoader _loader;
        private ChunkSplitMerge _splitMerge;
        private MeshRenderer _meshRenderer;

        private List<Material> _materials = new List<Material>();
        private CancellationTokenSource _splitCTS;

        private void Awake()
        {
            _loader = GetComponent<AsyncChunkLoader>();
            _splitMerge = GetComponent<ChunkSplitMerge>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.GetSharedMaterials(_materials);
        }

        private void OnDestroy()
        {
            CancelSplit();
        }

        public void Split()
        {
            if (_splitMerge.HasPieces)
                return;

            _loader.enabled = false;

            CancelSplit();
            _splitCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            SplitAsync(_splitCTS.Token).Forget(Debug.LogError);
        }

        public void CancelSplit()
        {
            if (_splitCTS == null)
                return;

            if (!_splitCTS.IsCancellationRequested)
                _splitCTS.Cancel();

            _splitCTS.Dispose();
            _splitCTS = null;
        }

        private async UniTask SplitAsync(CancellationToken ct = default)
        {
            if (_splitMerge.HasPieces)
                return;

            VoxelChunk chunk = await _loader.GetChunkVoxels();
            ct.ThrowIfCancellationRequested();

            if (chunk == null)
                return;

            _splitMerge.Split(chunk, _materials);
        }

        public void Merge()
        {
            CancelSplit();

            _loader.enabled = true;

            _splitMerge.Merge();
        }
    }
}
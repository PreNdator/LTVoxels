using Cysharp.Threading.Tasks;
using LedenevTV.Voxel;
using LedenevTV.Voxel.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AsyncChunkLoader : MonoBehaviour
    {
        [SerializeField]
        private AsyncBytesSource _byteSource;

        private IAsyncChunkProvider _chunkProvider;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;

        private Task<VoxelChunk> _getCloneTask;

        [Inject]
        private void Construct(IAsyncChunkProvider chunkProvider)
        {
            _chunkProvider = chunkProvider;
        }

        public async UniTask<VoxelChunk> GetChunkVoxels()
        {
            if (_getCloneTask == null)
            {
                _getCloneTask = _chunkProvider.GetChunkCloneAsync(_byteSource, destroyCancellationToken);
            }

            return await _getCloneTask;
        }

        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Start()
        {
            CreateMeshAsync(destroyCancellationToken).Forget(Debug.LogError);
        }

        private async UniTask CreateMeshAsync(CancellationToken ct)
        {
            Mesh mesh = await _chunkProvider.GetCachedChunkMeshAsync(_byteSource);
            if (!ct.IsCancellationRequested)
            {
                _meshFilter.sharedMesh = mesh;
                if (_meshCollider != null) _meshCollider.sharedMesh = mesh;
            }
        }

        protected virtual void OnEnable()
        {
            _meshRenderer.enabled = true;
            if (_meshCollider != null) _meshCollider.enabled = true;
        }

        protected virtual void OnDisable()
        {
            _meshRenderer.enabled = false;
            if (_meshCollider != null) _meshCollider.enabled = false;
        }

        protected virtual void OnDestroy()
        {
            if (_getCloneTask != null && _getCloneTask.IsCompletedSuccessfully)
            {
                VoxelChunk chunk = _getCloneTask.Result;
                chunk.Dispose();
            }
        }
    }
}
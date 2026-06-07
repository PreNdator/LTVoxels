using Cysharp.Threading.Tasks;
using LedenevTV.Voxel;
using LedenevTV.Voxel.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using R3;
#endif

namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AsyncChunkLoader : MonoBehaviour
#if UNITY_EDITOR
        , IEditorAsyncVoxelPreviewSource
#endif
    {
        [SerializeField]
        private AsyncBytesSource _byteSource;

#if UNITY_EDITOR
        private ReactiveProperty<IAsyncBytesSource> _editorAsyncByteSource;

        public ReadOnlyReactiveProperty<IAsyncBytesSource> EditorAsyncByteSource => EnsureEditorAsyncByteSource();
#endif

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureEditorAsyncByteSource().Value = _byteSource;
        }

        private ReactiveProperty<IAsyncBytesSource> EnsureEditorAsyncByteSource()
        {
            if (_editorAsyncByteSource == null)
            {
                _editorAsyncByteSource = new ReactiveProperty<IAsyncBytesSource>(_byteSource);
            }

            return _editorAsyncByteSource;
        }
#endif
    }
}

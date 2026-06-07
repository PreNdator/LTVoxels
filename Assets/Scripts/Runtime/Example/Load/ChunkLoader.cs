using LedenevTV.Voxel;
using LedenevTV.Voxel.Serialization;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using R3;
#endif

namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkLoader : MonoBehaviour
#if UNITY_EDITOR
        , IEditorVoxelPreviewSource
#endif
    {
        [SerializeField]
        private VoxelBytesAsset _byteSource;

#if UNITY_EDITOR
        private ReactiveProperty<IBytesSource> _editorByteSource;

        public ReadOnlyReactiveProperty<IBytesSource> EditorByteSource => EnsureEditorByteSource();
#endif

        private IChunkProvider _chunkProvider;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;

        private VoxelChunk _voxelClone;

        [Inject]
        private void Construct(IChunkProvider chunkProvider)
        {
            _chunkProvider = chunkProvider;
        }

        public VoxelChunk GetChunkVoxels()
        {
            if (_voxelClone == null)
            {
                _voxelClone = _chunkProvider.GetChunkClone(_byteSource);
            }

            return _voxelClone;
        }

        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Start()
        {
            CreateMesh();
        }

        private void CreateMesh()
        {
            Mesh mesh = _chunkProvider.GetCachedChunkMesh(_byteSource);

            _meshFilter.sharedMesh = mesh;
            if (_meshCollider != null) _meshCollider.sharedMesh = mesh;
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
            if (_voxelClone != null)
            {
                _voxelClone.Dispose();
            }

#if UNITY_EDITOR
            if (_editorByteSource != null)
            {
                _editorByteSource.Dispose();
                _editorByteSource = null;
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureEditorByteSource().Value = _byteSource;
        }

        private ReactiveProperty<IBytesSource> EnsureEditorByteSource()
        {
            if (_editorByteSource == null)
            {
                _editorByteSource = new ReactiveProperty<IBytesSource>(_byteSource);
            }

            return _editorByteSource;
        }
#endif
    }
}

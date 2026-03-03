using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using System.Collections;
using UnityEngine;
using Zenject;


namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(MeshFilter))]
    public abstract class ChunkEditAnimation : MonoBehaviour
    {
        [SerializeField, Range(0.001f, 10f)]
        private float _animationFramesDelay = 1;

        private Coroutine _animationCoroutine;
        private VoxelChunk _chunk;
        private Mesh _chunkMesh;

        private MeshFilter _meshFilter;

        private IVoxelMeshBuilder _voxelMeshBuilder;

        [Inject]
        private void Construct(IVoxelMeshBuilder voxelMeshBuilder)
        {
            _voxelMeshBuilder = voxelMeshBuilder;
        }

        protected virtual void Awake()
        {
            _chunkMesh = new Mesh();
            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.mesh = _chunkMesh;
            _chunk = new VoxelChunk();
        }

        protected virtual void Start()
        {
            Stop();
            Init();
        }

        public void Replay()
        {
            Stop();
            Init();
            _animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        public void Stop()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }


            ClearAnimationSettings();
        }

        public abstract void NextStep(VoxelChunk chunk);

        public abstract void ClearAnimationSettings();

        public abstract void RebuildChunk(VoxelChunk chunk);

        protected virtual void OnDestroy()
        {
            Stop();
            Destroy(_chunkMesh);
            if (_chunk != null)
            {
                _chunk.Dispose();
                _chunk = null;
            }
        }

        private void Init()
        {
            RebuildChunk(_chunk);
            _voxelMeshBuilder.RebuildMesh(_chunkMesh, _chunk, drawFacesOnBounds: true);
        }

        private IEnumerator AnimationCoroutine()
        {
            WaitForSeconds waitDelay = new WaitForSeconds(_animationFramesDelay);

            while (true)
            {
                yield return waitDelay;
                NextStep(_chunk);
                _voxelMeshBuilder.RebuildMesh(_chunkMesh, _chunk, drawFacesOnBounds: true);
            }
        }
    }
}


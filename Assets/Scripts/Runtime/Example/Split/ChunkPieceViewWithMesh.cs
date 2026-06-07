using LedenevTV.Voxel.Drawing;
using LedenevTV.Voxel.Collisions;
using LedenevTV.Voxel.Splitting;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LedenevTV.Runtime.Examples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class ChunkPieceViewWithMesh : ChunkPieceView
    {
        private Mesh _associatedMesh;

        private IVoxelMeshBuilder _voxelMeshBuilder;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        [SerializeField]
        private VoxelBoxColliderGroup _boxColliderGroup;

        [Inject]
        public void Construct(IVoxelMeshBuilder voxelMeshBuilder)
        {
            _voxelMeshBuilder = voxelMeshBuilder;
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public override void CreateMesh(ChunkPiece piece, List<Material> sharedMaterials)
        {
            _associatedMesh = new Mesh();

            transform.localPosition = piece.Offset;

            if (sharedMaterials != null) _meshRenderer.SetMaterials(sharedMaterials);

            _voxelMeshBuilder.RebuildMesh(_associatedMesh, piece.Chunk, drawFacesOnBounds: true);

            _meshFilter.sharedMesh = _associatedMesh;
            if (_boxColliderGroup != null)
            {
                _boxColliderGroup.Rebuild(piece.Chunk);
            }
        }

        protected override void Clear()
        {
            base.Clear();

            if (_associatedMesh != null)
            {
                if (_meshFilter != null && _meshFilter.sharedMesh == _associatedMesh)
                    _meshFilter.sharedMesh = null;

                if (_boxColliderGroup != null)
                    _boxColliderGroup.Clear();

                Destroy(_associatedMesh);
                _associatedMesh = null;
            }
        }
    }
}

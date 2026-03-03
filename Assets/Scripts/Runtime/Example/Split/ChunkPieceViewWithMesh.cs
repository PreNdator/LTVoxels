using LedenevTV.Voxel.Drawing;
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
        private MeshCollider _meshCollider;

        [Inject]
        public void Construct(IVoxelMeshBuilder voxelMeshBuilder)
        {
            _voxelMeshBuilder = voxelMeshBuilder;
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        public override void CreateMesh(ChunkPiece piece, List<Material> sharedMaterials)
        {
            _associatedMesh = new Mesh();

            transform.localPosition = piece.Offset;

            if (sharedMaterials != null) _meshRenderer.SetMaterials(sharedMaterials);

            _voxelMeshBuilder.RebuildMesh(_associatedMesh, piece.Chunk, drawFacesOnBounds: true);

            _meshFilter.sharedMesh = _associatedMesh;
            _meshCollider.sharedMesh = _associatedMesh;
        }

        protected override void Clear()
        {
            base.Clear();

            if (_associatedMesh != null)
            {
                if (_meshFilter != null && _meshFilter.sharedMesh == _associatedMesh)
                    _meshFilter.sharedMesh = null;

                if (_meshCollider != null && _meshCollider.sharedMesh == _associatedMesh)
                    _meshCollider.sharedMesh = null;

                Destroy(_associatedMesh);
                _associatedMesh = null;
            }
        }
    }
}
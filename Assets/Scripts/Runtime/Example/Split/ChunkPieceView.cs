using LedenevTV.Voxel.Splitting;
using System.Collections.Generic;
using UnityEngine;

namespace LedenevTV.Runtime.Examples
{
    public abstract class ChunkPieceView : MonoBehaviour
    {
        public ChunkPiece AssociatedPiece => _associatedPiece;
        private ChunkPiece _associatedPiece;

        public void Initialize(ChunkPiece piece, List<Material> sharedMaterials)
        {
            Clear();
            _associatedPiece = piece;
            CreateMesh(piece, sharedMaterials);
        }

        public abstract void CreateMesh(ChunkPiece piece, List<Material> sharedMaterials);

        protected virtual void Clear()
        {
            if (_associatedPiece.Chunk != null)
            {
                _associatedPiece.Chunk.Dispose();
                _associatedPiece.Chunk = null;
            }
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

    }
}
using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using LedenevTV.Voxel.Splitting;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LedenevTV.Runtime.Examples
{
    public sealed class ChunkSplitMerge : MonoBehaviour
    {
        public bool HasPieces => _hasPieces;

        private bool _hasPieces;
        private List<ChunkPieceView> _pieces = new List<ChunkPieceView>();

        private IChunkSplitter _chunkSplitter;
        private IVoxelMeshBuilder _meshBuilder;
        private ChunkPieceViewFactory _pieceFactory;


        [Inject]
        private void Construct(IChunkSplitter chunkSplitter, IVoxelMeshBuilder meshBuilder, ChunkPieceViewFactory pieceFactory)
        {
            _chunkSplitter = chunkSplitter;
            _meshBuilder = meshBuilder;
            _pieceFactory = pieceFactory;
        }

        private void OnDestroy()
        {
            ClearPieces();
        }

        public void Split(VoxelChunk chunk, List<Material> materials)
        {
            if (_hasPieces)
                return;

            if (chunk == null)
                return;

            _hasPieces = true;

            List<ChunkPiece> pieces = _chunkSplitter.Split(chunk);

            if (pieces == null || pieces.Count == 0)
                return;

            for (int i = 0; i < pieces.Count; i++)
            {
                ChunkPiece piece = pieces[i];

                ChunkPieceView pieceView = _pieceFactory.Create();

                pieceView.transform.SetParent(transform, false);

                pieceView.Initialize(piece, materials);

                _pieces.Add(pieceView);
            }

        }

        public void Merge()
        {
            ClearPieces();

            _hasPieces = false;
        }

        private void ClearPieces()
        {
            for (int i = 0; i < _pieces.Count; i++)
            {
                ChunkPieceView piece = _pieces[i];
                if (piece != null)
                    Destroy(piece.gameObject);
            }
            _pieces.Clear();
        }
    }
}
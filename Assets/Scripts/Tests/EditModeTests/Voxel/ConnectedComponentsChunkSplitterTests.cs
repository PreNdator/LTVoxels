using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using LedenevTV.Voxel.Splitting;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public class ConnectedComponentsChunkSplitterTests
    {
        [Test]
        public void Split_EmptyChunk_ReturnsNoPieces()
        {
            VoxelChunk chunk = new VoxelChunk(4);

            ConnectedComponentsChunkSplitter splitter = new ConnectedComponentsChunkSplitter(new NeighborVoxels6(), new MinCornerChunkSpace());
            List<ChunkPiece> pieces = splitter.Split(chunk);

            Assert.IsNotNull(pieces);
            Assert.AreEqual(0, pieces.Count);

            chunk.Dispose();
        }

        [Test]
        public void Split_SingleSolidVoxel_OnePiece_Size1x1x1_OffsetIsVoxelPos()
        {
            VoxelChunk chunk = new VoxelChunk(4);

            Vector3Int pos = new Vector3Int(1, 2, 3);
            bool setOk = chunk.TrySetVoxel(pos, VoxelType.Solid, 1);
            Assert.IsTrue(setOk);

            ConnectedComponentsChunkSplitter splitter = new ConnectedComponentsChunkSplitter(new NeighborVoxels6(), new MinCornerChunkSpace());
            List<ChunkPiece> pieces = splitter.Split(chunk);

            Assert.AreEqual(1, pieces.Count);

            ChunkPiece piece = pieces[0];

            int3 size = piece.Chunk.Size;
            Assert.AreEqual(1, size.x);
            Assert.AreEqual(1, size.y);
            Assert.AreEqual(1, size.z);

            Assert.AreEqual((Vector3)pos, piece.Offset);

            chunk.Dispose();
            piece.Chunk.Dispose();
        }

        [Test]
        public void Split_TwoSeparatedVoxels_TwoPieces_WithCorrectOffsets()
        {
            VoxelChunk chunk = new VoxelChunk(4);

            Vector3Int posA = new Vector3Int(0, 0, 0);
            Vector3Int posB = new Vector3Int(2, 0, 0);

            bool setA = chunk.TrySetVoxel(posA, VoxelType.Solid, 1);
            bool setB = chunk.TrySetVoxel(posB, VoxelType.Solid, 2);
            Assert.IsTrue(setA && setB);

            ConnectedComponentsChunkSplitter splitter = new ConnectedComponentsChunkSplitter(new NeighborVoxels6(), new MinCornerChunkSpace());
            List<ChunkPiece> pieces = splitter.Split(chunk);

            Assert.AreEqual(2, pieces.Count);

            bool foundA = false;
            bool foundB = false;

            for (int i = 0; i < pieces.Count; i++)
            {
                ChunkPiece piece = pieces[i];

                int3 size = piece.Chunk.Size;
                Assert.AreEqual(1, size.x);
                Assert.AreEqual(1, size.y);
                Assert.AreEqual(1, size.z);

                if (piece.Offset == posA)
                {
                    foundA = true;
                }
                else if (piece.Offset == posB)
                {
                    foundB = true;
                }

                piece.Chunk.Dispose();
            }

            Assert.IsTrue(foundA);
            Assert.IsTrue(foundB);

            chunk.Dispose();
        }
    }
}
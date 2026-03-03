using LedenevTV.Voxel;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public class VoxelChunkTests
    {
        private static void SetVoxel(VoxelChunk chunk, int index, VoxelType type, byte materialId)
        {
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;

            voxelTypes[index] = type;
            materialIds[index] = materialId;
        }

        [Test]
        public void Chunk_WithoutColors_ArraysSameLength_ColorsNotCreated()
        {
            using (VoxelChunk chunk = new VoxelChunk(4, useColors: false))
            {
                int expected = 4 * 4 * 4;

                Assert.AreEqual(expected, chunk.VoxelTypes.Length);
                Assert.AreEqual(expected, chunk.MaterialIds.Length);

                Assert.IsFalse(chunk.HasColors);
                Assert.IsFalse(chunk.Colors.IsCreated);
            }
        }

        [Test]
        public void Chunk_WithColors_ArraysSameLength_ColorsCreated()
        {
            using (VoxelChunk chunk = new VoxelChunk(4, useColors: true))
            {
                int expected = 4 * 4 * 4;

                Assert.AreEqual(expected, chunk.VoxelTypes.Length);
                Assert.AreEqual(expected, chunk.MaterialIds.Length);

                Assert.IsTrue(chunk.HasColors);
                Assert.IsTrue(chunk.Colors.IsCreated);
                Assert.AreEqual(expected, chunk.Colors.Length);
            }
        }

        [Test]
        public void Chunk_Rebuild_ResizesAndTogglesColors()
        {
            using (VoxelChunk chunk = new VoxelChunk(4, useColors: true))
            {
                Assert.IsTrue(chunk.HasColors);

                chunk.Rebuild(2, useColors: false);
                Assert.AreEqual(8, chunk.VoxelTypes.Length);
                Assert.IsFalse(chunk.HasColors);
                Assert.IsFalse(chunk.Colors.IsCreated);

                chunk.Rebuild(3, useColors: true);
                Assert.AreEqual(27, chunk.VoxelTypes.Length);
                Assert.IsTrue(chunk.HasColors);
                Assert.IsTrue(chunk.Colors.IsCreated);
            }
        }

        [Test]
        public void Chunk_NonCubicSize_Indexing_FirstLastAndInteriorMatchExpectedCoordinates()
        {
            Vector3Int chunkSize = new Vector3Int(4, 3, 2);

            using (var chunk = new VoxelChunk(chunkSize, useColors: false))
            {
                Vector3Int firstCoord = new Vector3Int(0, 0, 0);
                int firstIndex = ChunkIndexing.CoordToIndex(firstCoord, chunk.Size);
                Assert.AreEqual(0, firstIndex);

                Vector3Int firstRoundtrip = ChunkIndexing.IndexToCoord(firstIndex, chunkSize);
                Assert.AreEqual(firstCoord, firstRoundtrip);

                Vector3Int lastCoord = new Vector3Int(chunkSize.x - 1, chunkSize.y - 1, chunkSize.z - 1);
                int lastIndex = ChunkIndexing.CoordToIndex(lastCoord, chunk.Size);

                int expectedLastIndex = (chunkSize.x * chunkSize.y * chunkSize.z) - 1;
                Assert.AreEqual(expectedLastIndex, lastIndex);

                Vector3Int lastRoundtrip = ChunkIndexing.IndexToCoord(lastIndex, chunkSize);
                Assert.AreEqual(lastCoord, lastRoundtrip);

                Vector3Int interiorCoord = new Vector3Int(1, 1, 0);
                int interiorIndex = ChunkIndexing.CoordToIndex(interiorCoord, chunk.Size);

                Vector3Int interiorRoundtrip = ChunkIndexing.IndexToCoord(interiorIndex, chunkSize);
                Assert.AreEqual(interiorCoord, interiorRoundtrip);

                SetVoxel(chunk, firstIndex, VoxelType.Solid, 7);
                SetVoxel(chunk, interiorIndex, VoxelType.Transparent, 3);
                SetVoxel(chunk, lastIndex, VoxelType.Solid, 9);

                Assert.AreEqual(VoxelType.Solid, chunk.VoxelTypes[firstIndex]);
                Assert.AreEqual((byte)7, chunk.MaterialIds[firstIndex]);

                Assert.AreEqual(VoxelType.Transparent, chunk.VoxelTypes[interiorIndex]);
                Assert.AreEqual((byte)3, chunk.MaterialIds[interiorIndex]);

                Assert.AreEqual(VoxelType.Solid, chunk.VoxelTypes[lastIndex]);
                Assert.AreEqual((byte)9, chunk.MaterialIds[lastIndex]);
            }
        }
    }
}
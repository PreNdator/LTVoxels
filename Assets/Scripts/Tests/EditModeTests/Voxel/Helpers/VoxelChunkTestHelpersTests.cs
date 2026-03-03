using LedenevTV.Voxel;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace LedenevTV.Tests.Voxel
{
    public class VoxelChunkTestHelpersTests
    {
        [Test]
        public void CreateRandomChunk_SameSeed_CreatesIdenticalChunks_WithColors()
        {
            int3 size = new int3(6, 4, 3);
            const int seed = 123;
            const int batchSize = 64;

            VoxelChunk first = null;
            VoxelChunk second = null;

            try
            {
                first = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: true, seed: seed, batchSize: batchSize);
                second = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: true, seed: seed, batchSize: batchSize);

                VoxelChunkTestHelpers.AreEqual(first, second);
            }
            finally
            {
                first?.Dispose();
                second?.Dispose();
            }
        }

        [Test]
        public void CreateRandomChunk_SameSeed_CreatesIdenticalChunks_WithoutColors()
        {
            int3 size = new int3(6, 4, 3);
            const int seed = 123;
            const int batchSize = 64;

            VoxelChunk first = null;
            VoxelChunk second = null;

            try
            {
                first = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: false, seed: seed, batchSize: batchSize);
                second = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: false, seed: seed, batchSize: batchSize);

                VoxelChunkTestHelpers.AreEqual(first, second);
            }
            finally
            {
                first?.Dispose();
                second?.Dispose();
            }
        }

        [Test]
        public void CreateRandomChunk_DifferentSeeds_CreatesDifferentChunks()
        {
            int3 size = new int3(10, 6, 5);

            VoxelChunk first = null;
            VoxelChunk second = null;

            try
            {
                first = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: true, seed: 1, batchSize: 128);
                second = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: true, seed: 2, batchSize: 128);

                AssertChunksNotEqualAtLeastOneVoxel(first, second);
            }
            finally
            {
                first?.Dispose();
                second?.Dispose();
            }
        }

        [Test]
        public void FillWithRandomData_DoesNotGenerateOutside()
        {
            int3 size = new int3(12, 4, 7);
            const int seed = 777;

            VoxelChunk chunk = null;

            try
            {
                chunk = new VoxelChunk(size, useColors: true)
                {
                    BatchSize = 16
                };

                VoxelChunkTestHelpers.FillWithRandomData(chunk, seed);

                NativeArray<VoxelType> types = chunk.VoxelTypes;
                int count = chunk.VoxelsCount;

                for (int i = 0; i < count; i++)
                {
                    int3 coord = chunk.IndexToCoord(i);

                    Assert.AreNotEqual(
                        VoxelType.Outside,
                        types[i],
                        $"Outside voxel generated at index {i}, coord ({coord.x},{coord.y},{coord.z})."
                    );
                }
            }
            finally
            {
                chunk?.Dispose();
            }
        }

        private static void AssertChunksNotEqualAtLeastOneVoxel(VoxelChunk first, VoxelChunk second)
        {
            Assert.NotNull(first);
            Assert.NotNull(second);

            Assert.AreEqual(first.SizeV3Int, second.SizeV3Int, "Size mismatch (test setup).");
            Assert.AreEqual(first.VoxelsCount, second.VoxelsCount, "VoxelsCount mismatch (test setup).");
            Assert.AreEqual(first.HasColors, second.HasColors, "HasColors mismatch (test setup).");

            NativeArray<VoxelType> firstTypes = first.VoxelTypes;
            NativeArray<VoxelType> secondTypes = second.VoxelTypes;

            NativeArray<byte> firstMats = first.MaterialIds;
            NativeArray<byte> secondMats = second.MaterialIds;

            bool hasColors = first.HasColors;
            NativeArray<UnityEngine.Color32> firstCols = hasColors ? first.Colors : default;
            NativeArray<UnityEngine.Color32> secondCols = hasColors ? second.Colors : default;

            int count = first.VoxelsCount;

            for (int i = 0; i < count; i++)
            {
                if (firstTypes[i] != secondTypes[i] || firstMats[i] != secondMats[i] || (hasColors && !firstCols[i].Equals(secondCols[i])))
                {
                    return;
                }
            }

            Assert.Fail("Chunks are identical for different seeds (no differences found).");
        }
    }
}
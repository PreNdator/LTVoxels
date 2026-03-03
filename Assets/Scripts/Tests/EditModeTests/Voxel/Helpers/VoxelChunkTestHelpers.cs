using LedenevTV.Voxel;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public static class VoxelChunkTestHelpers
    {
        public static VoxelChunk CreateRandomChunk(int3 size, bool useColors, int seed = 123, int batchSize = 128)
        {
            var chunk = new VoxelChunk(size, useColors);
            chunk.BatchSize = batchSize;

            FillWithRandomData(chunk, seed);

            return chunk;
        }

        public static void FillWithRandomData(VoxelChunk chunk, int seed = 123)
        {
            if (!chunk.IsAllocated)
                throw new ArgumentException("Chunk is not created.", nameof(chunk));

            System.Random random = new System.Random(seed);

            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;

            NativeArray<Color32> colors = default;
            if (chunk.HasColors)
                colors = chunk.Colors;

            int voxelCount = chunk.VoxelsCount;


            for (int i = 0; i < voxelCount; i++)
            {
                voxelTypes[i] = (VoxelType)random.Next(0, 3);

                materialIds[i] = (byte)random.Next(0, 256);

                if (chunk.HasColors)
                {
                    byte r = (byte)random.Next(0, 256);
                    byte g = (byte)random.Next(0, 256);
                    byte b = (byte)random.Next(0, 256);
                    byte a = (byte)random.Next(0, 256);

                    colors[i] = new Color32(r, g, b, a);
                }
            }
        }

        public static void AreEqual(VoxelChunk expected, VoxelChunk actual)
        {
            Assert.NotNull(expected, "Expected chunk is null.");
            Assert.NotNull(actual, "Actual chunk is null.");

            Assert.AreEqual(expected.SizeV3Int, actual.SizeV3Int, "Size mismatch.");
            Assert.AreEqual(expected.BatchSize, actual.BatchSize, "BatchSize mismatch.");
            Assert.AreEqual(expected.HasColors, actual.HasColors, "HasColors mismatch.");
            Assert.AreEqual(expected.VoxelsCount, actual.VoxelsCount, "VoxelsCount mismatch.");

            int count = expected.VoxelsCount;

            NativeArray<VoxelType> expectedVoxelTypes = expected.VoxelTypes;
            NativeArray<byte> expectedMaterialIds = expected.MaterialIds;

            NativeArray<VoxelType> actualVoxelTypes = actual.VoxelTypes;
            NativeArray<byte> actualMaterialIds = actual.MaterialIds;

            bool hasColors = expected.HasColors;
            NativeArray<Color32> expectedColors = hasColors ? expected.Colors : default;
            NativeArray<Color32> actualColors = hasColors ? actual.Colors : default;

            for (int i = 0; i < count; i++)
            {
                int3 coord = expected.IndexToCoord(i);
                string pos = $"index {i}, coord ({coord.x},{coord.y},{coord.z})";

                Assert.AreEqual(expectedVoxelTypes[i], actualVoxelTypes[i], $"VoxelType mismatch at {pos}.");
                Assert.AreEqual(expectedMaterialIds[i], actualMaterialIds[i], $"MaterialId mismatch at {pos}.");

                if (hasColors)
                    Assert.AreEqual(expectedColors[i], actualColors[i], $"Color mismatch at {pos}.");
            }
        }
    }
}
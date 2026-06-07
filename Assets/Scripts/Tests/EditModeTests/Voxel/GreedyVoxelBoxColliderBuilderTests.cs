using LedenevTV.Voxel;
using LedenevTV.Voxel.Collisions;
using LedenevTV.Voxel.Drawing;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public sealed class GreedyVoxelBoxColliderBuilderTests
    {
        private readonly List<VoxelBoxColliderData> _results = new List<VoxelBoxColliderData>();

        [Test]
        public void Build_EmptyChunk_ReturnsNoBoxes()
        {
            using (VoxelChunk chunk = new VoxelChunk(4))
            {
                GreedyVoxelBoxColliderBuilder builder = CreateBuilder();

                builder.Build(chunk, _results);

                Assert.AreEqual(0, _results.Count);
            }
        }

        [Test]
        public void Build_SingleSolidVoxel_ReturnsOneUnitBox()
        {
            using (VoxelChunk chunk = new VoxelChunk(4))
            {
                chunk.TrySetVoxel(new Vector3Int(1, 2, 3), VoxelType.Solid, 1);
                GreedyVoxelBoxColliderBuilder builder = CreateBuilder();

                builder.Build(chunk, _results);

                Assert.AreEqual(1, _results.Count);
                AssertBox(_results[0], new Vector3(1.5f, 2.5f, 3.5f), Vector3.one);
            }
        }

        [Test]
        public void Build_SolidCuboid_ReturnsOneMergedBox()
        {
            using (VoxelChunk chunk = new VoxelChunk(new Vector3Int(4, 4, 4)))
            {
                FillCuboid(chunk, new Vector3Int(1, 1, 2), new Vector3Int(3, 2, 1));
                GreedyVoxelBoxColliderBuilder builder = CreateBuilder();

                builder.Build(chunk, _results);

                Assert.AreEqual(1, _results.Count);
                AssertBox(_results[0], new Vector3(2.5f, 2f, 2.5f), new Vector3(3, 2, 1));
            }
        }

        [Test]
        public void Build_LShape_CoversWithTwoBoxes()
        {
            using (VoxelChunk chunk = new VoxelChunk(3))
            {
                chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Solid, 1);
                chunk.TrySetVoxel(new Vector3Int(1, 0, 0), VoxelType.Solid, 1);
                chunk.TrySetVoxel(new Vector3Int(0, 1, 0), VoxelType.Solid, 1);
                GreedyVoxelBoxColliderBuilder builder = CreateBuilder();

                builder.Build(chunk, _results);

                Assert.AreEqual(2, _results.Count);
                AssertContainsBox(new Vector3(1f, 0.5f, 0.5f), new Vector3(2, 1, 1));
                AssertContainsBox(new Vector3(0.5f, 1.5f, 0.5f), Vector3.one);
            }
        }

        [Test]
        public void Build_SeparatedVoxels_ReturnsOneBoxPerIsland()
        {
            using (VoxelChunk chunk = new VoxelChunk(4))
            {
                chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Solid, 1);
                chunk.TrySetVoxel(new Vector3Int(3, 3, 3), VoxelType.Solid, 1);
                GreedyVoxelBoxColliderBuilder builder = CreateBuilder();

                builder.Build(chunk, _results);

                Assert.AreEqual(2, _results.Count);
                AssertContainsBox(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
                AssertContainsBox(new Vector3(3.5f, 3.5f, 3.5f), Vector3.one);
            }
        }

        [Test]
        public void Build_TransparentVoxel_UsesSettings()
        {
            using (VoxelChunk chunk = new VoxelChunk(2))
            {
                chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Transparent, 1);

                CreateBuilder(includeTransparentVoxels: false).Build(chunk, _results);
                Assert.AreEqual(0, _results.Count);

                CreateBuilder(includeTransparentVoxels: true).Build(chunk, _results);
                Assert.AreEqual(1, _results.Count);
            }
        }

        [Test]
        public void Build_CenterChunkSpace_OffsetsBoxByChunkPivot()
        {
            using (VoxelChunk chunk = new VoxelChunk(4))
            {
                chunk.TrySetVoxel(new Vector3Int(1, 2, 3), VoxelType.Solid, 1);
                GreedyVoxelBoxColliderBuilder builder = new GreedyVoxelBoxColliderBuilder(
                    new CenterChunkSpace(),
                    new VoxelColliderSettings());

                builder.Build(chunk, _results);

                Assert.AreEqual(1, _results.Count);
                AssertBox(_results[0], new Vector3(-0.5f, 0.5f, 1.5f), Vector3.one);
            }
        }

        private static GreedyVoxelBoxColliderBuilder CreateBuilder(bool includeTransparentVoxels = true)
        {
            return new GreedyVoxelBoxColliderBuilder(
                new MinCornerChunkSpace(),
                new VoxelColliderSettings(includeTransparentVoxels));
        }

        private static void FillCuboid(VoxelChunk chunk, Vector3Int min, Vector3Int size)
        {
            for (int z = min.z; z < min.z + size.z; z++)
            {
                for (int y = min.y; y < min.y + size.y; y++)
                {
                    for (int x = min.x; x < min.x + size.x; x++)
                    {
                        chunk.TrySetVoxel(new Vector3Int(x, y, z), VoxelType.Solid, 1);
                    }
                }
            }
        }

        private void AssertContainsBox(Vector3 expectedCenter, Vector3 expectedSize)
        {
            for (int i = 0; i < _results.Count; i++)
            {
                VoxelBoxColliderData box = _results[i];

                if (box.Center == expectedCenter && box.Size == expectedSize)
                    return;
            }

            Assert.Fail($"Expected box center {expectedCenter} size {expectedSize} was not generated.");
        }

        private static void AssertBox(VoxelBoxColliderData box, Vector3 expectedCenter, Vector3 expectedSize)
        {
            Assert.AreEqual(expectedCenter, box.Center);
            Assert.AreEqual(expectedSize, box.Size);
        }
    }
}

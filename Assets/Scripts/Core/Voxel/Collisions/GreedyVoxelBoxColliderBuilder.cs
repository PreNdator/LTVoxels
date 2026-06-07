using LedenevTV.Voxel.Drawing;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Collisions
{
    public sealed class GreedyVoxelBoxColliderBuilder : IVoxelBoxColliderBuilder
    {
        private readonly IChunkSpace _chunkSpace;
        private readonly VoxelColliderSettings _settings;

        public GreedyVoxelBoxColliderBuilder(IChunkSpace chunkSpace, VoxelColliderSettings settings)
        {
            _chunkSpace = chunkSpace ?? throw new ArgumentNullException(nameof(chunkSpace));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Build(VoxelChunk voxelChunk, List<VoxelBoxColliderData> results)
        {
            if (voxelChunk == null)
                throw new ArgumentNullException(nameof(voxelChunk));

            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (!voxelChunk.IsAllocated)
                return;

            int voxelCount = voxelChunk.VoxelsCount;
            NativeArray<byte> covered = new NativeArray<byte>(voxelCount, Allocator.Temp, NativeArrayOptions.ClearMemory);

            try
            {
                Build(voxelChunk, covered, results);
            }
            finally
            {
                if (covered.IsCreated)
                    covered.Dispose();
            }
        }

        private void Build(VoxelChunk voxelChunk, NativeArray<byte> covered, List<VoxelBoxColliderData> results)
        {
            int3 chunkSize = voxelChunk.Size;
            float3 pivot = _chunkSpace.GetPivot(voxelChunk);

            for (int z = 0; z < chunkSize.z; z++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int x = 0; x < chunkSize.x; x++)
                    {
                        int index = voxelChunk.CoordToIndex(x, y, z);

                        if (!CanUse(voxelChunk, covered, index))
                            continue;

                        int width = GrowX(voxelChunk, covered, x, y, z);
                        int height = GrowY(voxelChunk, covered, x, y, z, width);
                        int depth = GrowZ(voxelChunk, covered, x, y, z, width, height);

                        MarkCovered(voxelChunk, covered, x, y, z, width, height, depth);

                        int3 boxSize = new int3(width, height, depth);
                        float3 center = new float3(x, y, z) + (float3)boxSize * 0.5f - pivot;

                        results.Add(new VoxelBoxColliderData((Vector3)center, (Vector3)(float3)boxSize));
                    }
                }
            }
        }

        private int GrowX(VoxelChunk voxelChunk, NativeArray<byte> covered, int x, int y, int z)
        {
            int3 chunkSize = voxelChunk.Size;
            int width = 0;

            while (x + width < chunkSize.x)
            {
                int index = voxelChunk.CoordToIndex(x + width, y, z);
                if (!CanUse(voxelChunk, covered, index))
                    break;

                width++;
            }

            return width;
        }

        private int GrowY(VoxelChunk voxelChunk, NativeArray<byte> covered, int x, int y, int z, int width)
        {
            int3 chunkSize = voxelChunk.Size;
            int height = 1;

            while (y + height < chunkSize.y && CanUseRow(voxelChunk, covered, x, y + height, z, width))
            {
                height++;
            }

            return height;
        }

        private int GrowZ(VoxelChunk voxelChunk, NativeArray<byte> covered, int x, int y, int z, int width, int height)
        {
            int3 chunkSize = voxelChunk.Size;
            int depth = 1;

            while (z + depth < chunkSize.z && CanUseLayer(voxelChunk, covered, x, y, z + depth, width, height))
            {
                depth++;
            }

            return depth;
        }

        private bool CanUseRow(VoxelChunk voxelChunk, NativeArray<byte> covered, int x, int y, int z, int width)
        {
            int startIndex = voxelChunk.CoordToIndex(x, y, z);

            for (int dx = 0; dx < width; dx++)
            {
                if (!CanUse(voxelChunk, covered, startIndex + dx))
                    return false;
            }

            return true;
        }

        private bool CanUseLayer(
            VoxelChunk voxelChunk,
            NativeArray<byte> covered,
            int x,
            int y,
            int z,
            int width,
            int height)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (!CanUseRow(voxelChunk, covered, x, y + dy, z, width))
                    return false;
            }

            return true;
        }

        private void MarkCovered(
            VoxelChunk voxelChunk,
            NativeArray<byte> covered,
            int x,
            int y,
            int z,
            int width,
            int height,
            int depth)
        {
            for (int dz = 0; dz < depth; dz++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    int startIndex = voxelChunk.CoordToIndex(x, y + dy, z + dz);

                    for (int dx = 0; dx < width; dx++)
                    {
                        covered[startIndex + dx] = 1;
                    }
                }
            }
        }

        private bool CanUse(VoxelChunk voxelChunk, NativeArray<byte> covered, int index)
        {
            return covered[index] == 0 && _settings.IsCollidable(voxelChunk.VoxelTypes[index]);
        }
    }
}

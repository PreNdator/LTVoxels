using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask of indices for voxels inside an axis-aligned box (inclusive bounds),
    /// clamped to the chunk boundaries.
    /// </summary>
    public readonly struct CubeMaskCreator : IChunkMaskCreator
    {
        public Vector3Int Min { get; }
        public Vector3Int Max { get; }

        public CubeMaskCreator(Vector3Int min, Vector3Int max)
        {
            Min = min;
            Max = max;
        }

        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            int3 size = voxelChunk.Size;

            int minX = math.clamp(Min.x, 0, size.x - 1);
            int minY = math.clamp(Min.y, 0, size.y - 1);
            int minZ = math.clamp(Min.z, 0, size.z - 1);

            int maxX = math.clamp(Max.x, 0, size.x - 1);
            int maxY = math.clamp(Max.y, 0, size.y - 1);
            int maxZ = math.clamp(Max.z, 0, size.z - 1);

            if (minX > maxX || minY > maxY || minZ > maxZ)
                return new NativeArray<int>(0, Allocator.TempJob);

            int countX = maxX - minX + 1;
            int countY = maxY - minY + 1;
            int countZ = maxZ - minZ + 1;

            int total = countX * countY * countZ;
            NativeArray<int> indices = new NativeArray<int>(total, Allocator.TempJob);

            int i = 0;

            for (int z = minZ; z <= maxZ; z++)
            {
                int zBase = size.x * size.y * z;
                for (int y = minY; y <= maxY; y++)
                {
                    int yBase = size.x * y + zBase;
                    for (int x = minX; x <= maxX; x++)
                    {
                        indices[i++] = x + yBase;
                    }
                }
            }

            return indices;
        }
    }

}


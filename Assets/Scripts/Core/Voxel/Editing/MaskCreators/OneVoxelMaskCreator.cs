using Unity.Collections;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask that selects a single voxel by index (or an empty mask if out of bounds).
    /// </summary>
    public struct OneVoxelMaskCreator : IChunkMaskCreator
    {
        public int Index { get; }

        public OneVoxelMaskCreator(int index)
        {
            Index = index;
        }

        public OneVoxelMaskCreator(Vector3Int coord, VoxelChunk voxelChunk)
        {
            Index = voxelChunk.CoordToIndex(coord);
        }

        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            int voxelCount = voxelChunk.VoxelTypes.Length;

            NativeArray<int> voxelMask = default;

            if (Index < voxelCount && Index >= 0)
            {
                voxelMask = new NativeArray<int>(1, Allocator.TempJob);
                voxelMask[0] = Index;
            }
            else
            {
                voxelMask = new NativeArray<int>(0, Allocator.TempJob);
            }

            return voxelMask;
        }
    }

}


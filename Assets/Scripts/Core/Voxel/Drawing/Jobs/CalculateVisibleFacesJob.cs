using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace LedenevTV.Voxel.Drawing
{
    [BurstCompile]
    internal struct CalculateVisibleFacesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<VoxelType> VoxelTypes;
        [ReadOnly]
        public int3 ChunkSize;
        [ReadOnly]
        public VoxelType OutOfBoundsVoxel;

        [WriteOnly]
        public NativeArray<byte> VisibleFacesMask;
        [WriteOnly]
        public NativeArray<byte> VisibleFacesCount;

        public void Execute(int index)
        {
            VoxelType currType = VoxelTypes[index];

            if (currType == VoxelType.Empty)
            {
                VisibleFacesMask[index] = 0;
                VisibleFacesCount[index] = 0;
                return;
            }

            int sizeX = ChunkSize.x;
            int sizeY = ChunkSize.y;
            int sizeZ = ChunkSize.z;

            int strideY = sizeX;
            int strideZ = sizeX * sizeY;

            int3 size = ChunkIndexing.IndexToCoord(index, ChunkSize);

            VoxelType neighborPosX = (size.x + 1 < sizeX) ? VoxelTypes[index + 1] : OutOfBoundsVoxel;
            VoxelType neighborNegX = (size.x > 0) ? VoxelTypes[index - 1] : OutOfBoundsVoxel;

            VoxelType neighborPosY = (size.y + 1 < sizeY) ? VoxelTypes[index + strideY] : OutOfBoundsVoxel;
            VoxelType neighborNegY = (size.y > 0) ? VoxelTypes[index - strideY] : OutOfBoundsVoxel;

            VoxelType neighborPosZ = (size.z + 1 < sizeZ) ? VoxelTypes[index + strideZ] : OutOfBoundsVoxel;
            VoxelType neighborNegZ = (size.z > 0) ? VoxelTypes[index - strideZ] : OutOfBoundsVoxel;

            byte mask = 0;

            if (currType == VoxelType.Solid)
            {
                if (IsFaceVisibleForSolid(neighborPosX)) mask |= VoxelFaceMask.PosX;
                if (IsFaceVisibleForSolid(neighborNegX)) mask |= VoxelFaceMask.NegX;
                if (IsFaceVisibleForSolid(neighborPosY)) mask |= VoxelFaceMask.PosY;
                if (IsFaceVisibleForSolid(neighborNegY)) mask |= VoxelFaceMask.NegY;
                if (IsFaceVisibleForSolid(neighborPosZ)) mask |= VoxelFaceMask.PosZ;
                if (IsFaceVisibleForSolid(neighborNegZ)) mask |= VoxelFaceMask.NegZ;
            }
            else if (currType == VoxelType.Transparent)
            {
                if (IsFaceVisibleForTransparent(neighborPosX)) mask |= VoxelFaceMask.PosX;
                if (IsFaceVisibleForTransparent(neighborNegX)) mask |= VoxelFaceMask.NegX;
                if (IsFaceVisibleForTransparent(neighborPosY)) mask |= VoxelFaceMask.PosY;
                if (IsFaceVisibleForTransparent(neighborNegY)) mask |= VoxelFaceMask.NegY;
                if (IsFaceVisibleForTransparent(neighborPosZ)) mask |= VoxelFaceMask.PosZ;
                if (IsFaceVisibleForTransparent(neighborNegZ)) mask |= VoxelFaceMask.NegZ;
            }
            else
            {
                mask = 0;
            }

            VisibleFacesMask[index] = mask;

            int bitCount = math.countbits((int)mask);
            VisibleFacesCount[index] = (byte)bitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFaceVisibleForSolid(VoxelType neighbor)
        {
            return neighbor == VoxelType.Empty || neighbor == VoxelType.Transparent;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFaceVisibleForTransparent(VoxelType neighbor)
        {
            return neighbor == VoxelType.Empty;
        }
    }
}
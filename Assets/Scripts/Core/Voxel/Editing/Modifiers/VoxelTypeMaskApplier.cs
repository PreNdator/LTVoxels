using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    public sealed class VoxelTypeMaskApplier : IVoxelTypeMaskApplier
    {
        public void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, VoxelType voxelType) where TMaskCreator : struct, IChunkMaskCreator
        {
            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                int maskLength = mask.Length;
                NativeArray<VoxelType> voxelTypes = voxelChunk.VoxelTypes;

                for (int i = 0; i < maskLength; i++)
                {
                    int voxelIndex = mask[i];
                    voxelTypes[voxelIndex] = voxelType;
                }
            }
        }
    }
}


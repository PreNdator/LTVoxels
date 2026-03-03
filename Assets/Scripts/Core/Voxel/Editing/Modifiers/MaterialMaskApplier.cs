using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    public sealed class MaterialMaskApplier : IMaterialMaskApplier
    {
        public void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, byte materialId) where TMaskCreator : struct, IChunkMaskCreator
        {
            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                int maskLength = mask.Length;
                NativeArray<byte> materialIds = voxelChunk.MaterialIds;

                for (int i = 0; i < maskLength; i++)
                {
                    int voxelIndex = mask[i];
                    materialIds[voxelIndex] = materialId;
                }
            }
        }
    }
}


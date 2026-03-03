using Unity.Collections;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    public sealed class ChunkMaskApplier : IChunkMaskApplier
    {
        public void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, MaskApplySettings settings) where TMaskCreator : struct, IChunkMaskCreator
        {
            bool shouldModifyAnything =
                settings.IsModifyVoxelType ||
                settings.IsModifyMaterialId ||
                (settings.IsModifyColors && voxelChunk.HasColors);

            if (!shouldModifyAnything)
                return;


            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                int maskLength = mask.Length;

                if (settings.IsModifyVoxelType)
                {
                    VoxelType targetType = settings.VoxType;
                    NativeArray<VoxelType> voxelTypes = voxelChunk.VoxelTypes;

                    for (int i = 0; i < maskLength; i++)
                    {
                        int voxelIndex = mask[i];
                        voxelTypes[voxelIndex] = targetType;
                    }
                }

                if (settings.IsModifyMaterialId)
                {
                    byte targetMaterialId = settings.MaterialId;
                    NativeArray<byte> materialIds = voxelChunk.MaterialIds;

                    for (int i = 0; i < maskLength; i++)
                    {
                        int voxelIndex = mask[i];
                        materialIds[voxelIndex] = targetMaterialId;
                    }
                }

                if (settings.IsModifyColors && voxelChunk.HasColors)
                {
                    Color32 targetColor = settings.Color;
                    NativeArray<Color32> colors = voxelChunk.Colors;

                    for (int i = 0; i < maskLength; i++)
                    {
                        int voxelIndex = mask[i];
                        colors[voxelIndex] = targetColor;
                    }
                }
            }
        }
    }
}


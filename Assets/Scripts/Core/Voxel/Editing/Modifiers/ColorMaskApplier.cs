using Unity.Collections;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    public sealed class ColorMaskApplier : IColorMaskApplier
    {
        public void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, Color32 color) where TMaskCreator : struct, IChunkMaskCreator
        {
            if (!voxelChunk.HasColors)
                return;

            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                int maskLength = mask.Length;

                NativeArray<Color32> colors = voxelChunk.Colors;

                for (int i = 0; i < maskLength; i++)
                {
                    int voxelIndex = mask[i];
                    colors[voxelIndex] = color;
                }
            }
        }
    }
}


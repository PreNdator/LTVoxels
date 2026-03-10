using System;
using Unity.Collections;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    public sealed class ChunkBreaker : IChunkBreaker
    {
        public NativeList<int> Break<TMaskCreator>(
            DamagableChunk voxelChunk,
            TMaskCreator maskCreator,
            float damage,
            Allocator allocator = Allocator.Temp)
            where TMaskCreator : struct, IChunkMaskCreator
        {
            NativeList<int> brokenVoxels = new NativeList<int>(allocator);

            if (damage <= 0f)
                return brokenVoxels;

            if (!voxelChunk.HasColors)
                throw new InvalidOperationException("Chunk must have colors because alpha is used as voxel health.");

            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                NativeArray<float> damageMultipliers = voxelChunk.DamageMultiplier;
                NativeArray<Color32> colors = voxelChunk.Colors;
                NativeArray<VoxelType> voxelTypes = voxelChunk.VoxelTypes;

                if (brokenVoxels.Capacity < mask.Length)
                    brokenVoxels.Capacity = mask.Length;

                for (int i = 0; i < mask.Length; i++)
                {
                    int voxelIndex = mask[i];

                    if (voxelTypes[voxelIndex] == VoxelType.Empty)
                        continue;

                    float finalDamage = damage * damageMultipliers[voxelIndex];
                    if (finalDamage <= 0f)
                        continue;

                    Color32 color = colors[voxelIndex];
                    float newAlpha = color.a - finalDamage;

                    if (newAlpha <= 0f)
                    {
                        color.a = 0;
                        colors[voxelIndex] = color;
                        voxelTypes[voxelIndex] = VoxelType.Empty;
                        brokenVoxels.Add(voxelIndex);
                    }
                    else
                    {
                        color.a = (byte)Mathf.Clamp(Mathf.RoundToInt(newAlpha), 0, 255);
                        colors[voxelIndex] = color;
                    }
                }
            }

            return brokenVoxels;
        }
    }
}


using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    public sealed class ChunkDamageApplier : IChunkDamageApplier
    {
        public void Apply<TMaskCreator>(DamagableChunk voxelChunk, TMaskCreator maskCreator, float damageMultiplier)
            where TMaskCreator : struct, IChunkMaskCreator
        {
            using (NativeArray<int> mask = maskCreator.CreateNewMask(voxelChunk))
            {
                int maskLength = mask.Length;
                NativeArray<float> multipliers = voxelChunk.DamageMultiplier;

                for (int i = 0; i < maskLength; i++)
                {
                    int voxelIndex = mask[i];
                    multipliers[voxelIndex] = damageMultiplier;
                }
            }
        }
    }
}


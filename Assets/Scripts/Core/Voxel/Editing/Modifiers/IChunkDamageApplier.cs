namespace LedenevTV.Voxel.Editing
{
    public interface IChunkDamageApplier
    {
        /// <summary>
        /// Applies the same damage multiplier to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// </summary>
        void Apply<TMaskCreator>(DamagableChunk voxelChunk, TMaskCreator maskCreator, float damageMultiplier) where TMaskCreator : struct, IChunkMaskCreator;
    }
}
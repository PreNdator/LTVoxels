namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Applies multiple voxel property changes using a chunk mask.
    /// </summary>
    public interface IChunkMaskApplier
    {
        /// <summary>
        /// Applies modifications from <paramref name="settings"/> to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// </summary>
        void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, MaskApplySettings settings) where TMaskCreator : struct, IChunkMaskCreator;
    }
}


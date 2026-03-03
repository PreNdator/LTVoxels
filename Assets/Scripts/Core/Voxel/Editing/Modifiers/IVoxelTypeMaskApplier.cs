namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Applies a voxel type to masked voxels.
    /// </summary>
    public interface IVoxelTypeMaskApplier
    {
        /// <summary>
        /// Applies <paramref name="voxelType"/> to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// </summary>
        void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, VoxelType voxelType) where TMaskCreator : struct, IChunkMaskCreator;
    }
}


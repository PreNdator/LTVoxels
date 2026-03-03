namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Applies a material Id to masked voxels.
    /// </summary>
    public interface IMaterialMaskApplier
    {
        /// <summary>
        /// Applies <paramref name="materialId"/> to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// </summary>
        void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, byte materialId) where TMaskCreator : struct, IChunkMaskCreator;
    }
}


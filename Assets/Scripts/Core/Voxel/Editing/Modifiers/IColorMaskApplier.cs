using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Applies a color to masked voxels.
    /// </summary>
    public interface IColorMaskApplier
    {
        /// <summary>
        /// Applies <paramref name="color"/> to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// </summary>
        void Apply<TMaskCreator>(VoxelChunk voxelChunk, TMaskCreator maskCreator, Color32 color) where TMaskCreator : struct, IChunkMaskCreator;
    }
}


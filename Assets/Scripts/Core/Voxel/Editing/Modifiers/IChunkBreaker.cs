using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    public interface IChunkBreaker
    {
        /// <summary>
        /// Applies damage to voxels selected by the mask produced by <paramref name="maskCreator"/>.
        /// Damage is multiplied by per-voxel damage multiplier and subtracted from voxel alpha.
        /// If alpha reaches 0, voxel becomes Empty.
        /// Returns indices of broken voxels.
        /// </summary>
        NativeList<int> Break<TMaskCreator>(
            DamagableChunk voxelChunk,
            TMaskCreator maskCreator,
            float damage,
            Allocator allocator = Allocator.Temp)
            where TMaskCreator : struct, IChunkMaskCreator;
    }
}


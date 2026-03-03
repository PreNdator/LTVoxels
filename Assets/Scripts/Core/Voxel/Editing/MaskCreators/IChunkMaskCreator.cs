using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    public interface IChunkMaskCreator
    {
        /// <summary>
        /// Creates a new mask as a list of voxel indices selected within <paramref name="voxelChunk"/>; the returned array must be disposed by the caller.
        /// </summary>
        NativeArray<int> CreateNewMask(VoxelChunk voxelChunk);
    }
}


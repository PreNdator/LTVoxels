using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask by copying indices from <see cref="Mask"/>.
    /// </summary>
    public struct ArrayCopyMaskCreator : IChunkMaskCreator
    {
        public NativeArray<int> Mask { get; }

        public ArrayCopyMaskCreator(NativeArray<int> mask)
        {
            Mask = mask;
        }

        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            NativeArray<int> copy = new NativeArray<int>(Mask.Length, Allocator.TempJob);
            NativeArray<int>.Copy(Mask, copy);
            return copy;
        }
    }

}


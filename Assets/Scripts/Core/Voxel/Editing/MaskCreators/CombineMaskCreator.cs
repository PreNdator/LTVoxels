using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask by concatenating masks produced by <see cref="First"/> and <see cref="Second"/>.
    /// </summary>
    public struct CombineMaskCreator : IChunkMaskCreator
    {
        IChunkMaskCreator First { get; }
        IChunkMaskCreator Second { get; }

        public CombineMaskCreator(IChunkMaskCreator first, IChunkMaskCreator second)
        {
            First = first;
            Second = second;
        }

        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            using NativeArray<int> firstArray = First.CreateNewMask(voxelChunk);
            using NativeArray<int> secondArray = Second.CreateNewMask(voxelChunk);

            int firstLen = firstArray.IsCreated ? firstArray.Length : 0;
            int secondLen = secondArray.IsCreated ? secondArray.Length : 0;

            int totalLen = firstLen + secondLen;

            if (totalLen == 0)
                return new NativeArray<int>(0, Allocator.TempJob);

            NativeArray<int> result = new NativeArray<int>(totalLen, Allocator.TempJob);

            if (firstLen > 0)
                NativeArray<int>.Copy(firstArray, 0, result, 0, firstLen);

            if (secondLen > 0)
                NativeArray<int>.Copy(secondArray, 0, result, firstLen, secondLen);

            return result;
        }
    }

}


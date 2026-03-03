using Unity.Collections;

namespace LedenevTV.Voxel.Editing
{
    internal static class MaskConverter
    {
        /// <summary>
        /// Converts <paramref name="byteMask"/> to an indices mask by returning indices where the value is non-zero.
        /// </summary>
        public static NativeArray<int> ByteMaskToIndices(NativeArray<byte> byteMask, Allocator allocator)
        {
            int voxelCount = byteMask.Length;

            int selectedCount = 0;
            for (int i = 0; i < voxelCount; i++)
            {
                if (byteMask[i] != 0)
                    selectedCount++;
            }

            NativeArray<int> indicesMask = new NativeArray<int>(selectedCount, allocator);

            int writeIndex = 0;
            for (int i = 0; i < voxelCount; i++)
            {
                if (byteMask[i] != 0)
                {
                    indicesMask[writeIndex] = i;
                    writeIndex++;
                }
            }

            return indicesMask;
        }
    }
}


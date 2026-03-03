using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel
{
    /// <summary>
    /// Converts between 3D chunk coordinates and a 1D linear index.
    /// </summary>
    [BurstCompile]
    public static class ChunkIndexing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CoordToIndex(int x, int y, int z, int3 size)
        {
            return x + size.x * (y + size.y * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CoordToIndex(int3 coord, int3 size)
        {
            return CoordToIndex(coord.x, coord.y, coord.z, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CoordToIndex(Vector3Int coord, int3 size)
        {
            return CoordToIndex(coord.x, coord.y, coord.z, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IndexToCoord(int index, int sizeX, int sizeY, out int x, out int y, out int z)
        {
            x = index % sizeX;
            y = (index / sizeX) % sizeY;
            z = index / (sizeX * sizeY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int IndexToCoord(int index, Vector3Int size)
        {
            IndexToCoord(index, size.x, size.y, out int x, out int y, out int z);
            return new Vector3Int(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToCoord(int index, int3 size)
        {
            IndexToCoord(index, size.x, size.y, out int x, out int y, out int z);
            return new int3(x, y, z);
        }

        /// <summary>
        /// Returns true if <paramref name="index"/> is within the valid range for a chunk of the given <paramref name="size"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(int index, int3 size)
        {
            if (index < 0) return false;

            int volume = size.x * size.y * size.z;
            return index < volume;
        }
    }
}
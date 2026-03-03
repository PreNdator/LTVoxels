using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Extensions.Span
{
    public static class VoxSpanExtensions
    {
        /// <summary>
        /// Reads a MagicaVoxel SIZE chunk payload (3x int32) and advances the position.
        /// </summary>
        /// <exception cref="InvalidDataException">
        /// Thrown when the dimensions are invalid.
        /// </exception>
        public static int3 ReadVoxSize(this ReadOnlySpan<byte> data, ref int position)
        {
            int sizeX = data.ReadInt32(ref position);
            int sizeY = data.ReadInt32(ref position);
            int sizeZ = data.ReadInt32(ref position);

            if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
                throw new InvalidDataException($"Invalid SIZE chunk dimensions: {sizeX}x{sizeY}x{sizeZ}");

            return new int3(sizeX, sizeY, sizeZ);
        }

        /// <summary>
        /// Reads a MagicaVoxel XYZI chunk payload and appends voxels to the list.
        /// </summary>
        public static void ReadVoxXyzi(this ReadOnlySpan<byte> data, ref int position,
            List<(byte x, byte y, byte z, byte colorIndex)> voxels)
        {
            int numVoxels = data.ReadInt32(ref position);
            if (numVoxels <= 0)
                return;

            for (int i = 0; i < numVoxels; i++)
            {
                byte x = data.ReadByte(ref position);
                byte y = data.ReadByte(ref position);
                byte z = data.ReadByte(ref position);
                byte colorIndex = data.ReadByte(ref position);

                voxels.Add((x, y, z, colorIndex));
            }
        }

        /// <summary>
        /// Reads a MagicaVoxel RGBA chunk payload (256 colors) and advances the position.
        /// </summary>
        public static Color32[] ReadVoxRgbaPalette(this ReadOnlySpan<byte> data, ref int position)
        {
            const int PaletteSize = 256;
            Color32[] palette = new Color32[PaletteSize];

            for (int i = 0; i < PaletteSize; i++)
            {
                byte r = data.ReadByte(ref position);
                byte g = data.ReadByte(ref position);
                byte b = data.ReadByte(ref position);
                byte a = data.ReadByte(ref position);

                palette[i] = new Color32(r, g, b, a);
            }

            return palette;
        }
    }
}
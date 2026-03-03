using LedenevTV.Extensions.Span;
using System;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class VoxchImportExport : IChunkImporter, IChunkExporter
    {
        private static readonly byte[] FileSignature = { (byte)'V', (byte)'O', (byte)'X', (byte)'C', (byte)'H' };
        public readonly int Version = 1;
        private const byte HasColorsFlag = 1;

        public bool CanImport(ReadOnlySpan<byte> data)
        {
            if (!HasMinimumHeaderLength(data.Length))
                return false;

            if (!HasValidSignature(data))
                return false;

            if (!HasValidVersion(data))
                return false;

            return true;
        }

        public VoxelChunk FromBytes(ReadOnlySpan<byte> data)
        {
            if (!CanImport(data))
                throw new InvalidDataException("Data is not a valid VOXCH chunk.");

            int position = 0;
            bool hasColors;
            int3 size;
            int voxelCount;
            int batchSize;

            ReadHeader(data, ref position, out hasColors, out size, out voxelCount, out batchSize);

            VoxelChunk chunk = new VoxelChunk(size, hasColors);
            chunk.BatchSize = batchSize;

            ReadVoxels(data, ref position, chunk, hasColors, voxelCount);

            return chunk;
        }

        public byte[] ToBytes(VoxelChunk chunk)
        {
            if (chunk == null || !chunk.IsAllocated)
            {
                return null;
            }

            int3 size = chunk.Size;
            int voxelCount = size.x * size.y * size.z;
            bool hasColors = chunk.HasColors;

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                WriteHeader(binaryWriter, size, hasColors, chunk.BatchSize);
                WriteVoxels(binaryWriter, chunk, voxelCount, hasColors);

                binaryWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        private bool HasMinimumHeaderLength(int length)
        {
            int minimumLength = FileSignature.Length + sizeof(int);
            return length >= minimumLength;
        }

        private bool HasValidSignature(ReadOnlySpan<byte> data)
        {
            if (data.Length < FileSignature.Length)
                return false;

            for (int i = 0; i < FileSignature.Length; i++)
            {
                if (data[i] != FileSignature[i])
                    return false;
            }

            return true;
        }

        private bool HasValidVersion(ReadOnlySpan<byte> data)
        {
            int offset = FileSignature.Length;
            if (data.Length < offset + sizeof(int))
                return false;

            int version = BitConverter.ToInt32(data.Slice(offset, sizeof(int)));
            return version == Version;
        }

        private int ComputeVoxelCount(int sizeX, int sizeY, int sizeZ)
        {
            if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
                throw new InvalidDataException($"Invalid chunk dimensions: {sizeX}x{sizeY}x{sizeZ}");

            long count = (long)sizeX * sizeY * sizeZ;
            if (count > int.MaxValue)
                throw new InvalidDataException($"Chunk is too large: {sizeX}x{sizeY}x{sizeZ}");

            return (int)count;
        }

        private void ReadHeader(ReadOnlySpan<byte> data, ref int position, out bool hasColors, out int3 size, out int voxelCount, out int batchSize)
        {
            data.Skip(ref position, FileSignature.Length + sizeof(int));

            int colorsChanels = 4;
            int dimentions = 3;

            byte flags = data.ReadByte(ref position);
            hasColors = (flags & HasColorsFlag) != 0;

            int sizeX = data.ReadInt32(ref position);
            int sizeY = data.ReadInt32(ref position);
            int sizeZ = data.ReadInt32(ref position);

            batchSize = data.ReadInt32(ref position);

            voxelCount = ComputeVoxelCount(sizeX, sizeY, sizeZ);
            size = new int3(sizeX, sizeY, sizeZ);

            int headerSize = FileSignature.Length + sizeof(int) + sizeof(byte) + sizeof(int) * dimentions + sizeof(int);

            int perVoxelBytes = sizeof(byte) + sizeof(byte);
            if (hasColors)
                perVoxelBytes += colorsChanels;

            long minimumExpectedLength = headerSize + (long)perVoxelBytes * voxelCount;
            if (data.Length < minimumExpectedLength)
                throw new InvalidDataException("VOXCH data is truncated or corrupted.");
        }

        public static void ReadVoxels(ReadOnlySpan<byte> data, ref int position, VoxelChunk chunk, bool hasColors, int voxelCount)
        {
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;
            NativeArray<Color32> colors = hasColors ? chunk.Colors : default;

            for (int i = 0; i < voxelCount; i++)
            {
                byte voxelTypeByte = data.ReadByte(ref position);

                voxelTypes[i] = (VoxelType)voxelTypeByte;

                materialIds[i] = data.ReadByte(ref position);

                if (hasColors)
                {
                    byte r = data.ReadByte(ref position);
                    byte g = data.ReadByte(ref position);
                    byte b = data.ReadByte(ref position);
                    byte a = data.ReadByte(ref position);
                    colors[i] = new Color32(r, g, b, a);
                }
            }
        }

        private void WriteHeader(BinaryWriter binaryWriter, int3 size, bool hasColors, int batchSize)
        {
            binaryWriter.Write(FileSignature);
            binaryWriter.Write(Version);

            byte flags = 0;
            if (hasColors)
                flags |= HasColorsFlag;
            binaryWriter.Write(flags);

            binaryWriter.Write(size.x);
            binaryWriter.Write(size.y);
            binaryWriter.Write(size.z);

            binaryWriter.Write(batchSize);
        }

        public static void WriteVoxels(BinaryWriter binaryWriter, VoxelChunk chunk, int voxelCount, bool hasColors)
        {
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;
            NativeArray<Color32> colors = hasColors ? chunk.Colors : default;

            for (int i = 0; i < voxelCount; i++)
            {
                binaryWriter.Write((byte)voxelTypes[i]);
                binaryWriter.Write(materialIds[i]);

                if (hasColors)
                {
                    Color32 color = colors[i];
                    binaryWriter.Write(color.r);
                    binaryWriter.Write(color.g);
                    binaryWriter.Write(color.b);
                    binaryWriter.Write(color.a);
                }
            }
        }
    }
}
using LedenevTV.Extensions.Span;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace LedenevTV.Voxel.Serialization
{
    public sealed class MagicaVoxelVoxImporter : IChunkImporter
    {
        private static readonly byte[] FileSignature = { (byte)'V', (byte)'O', (byte)'X', (byte)' ' };
        private const int SupportedVersion = 150;

        private const int ChunkId_MAIN = 'M' | ('A' << 8) | ('I' << 16) | ('N' << 24);
        private const int ChunkId_PACK = 'P' | ('A' << 8) | ('C' << 16) | ('K' << 24);
        private const int ChunkId_SIZE = 'S' | ('I' << 8) | ('Z' << 16) | ('E' << 24);
        private const int ChunkId_XYZI = 'X' | ('Y' << 8) | ('Z' << 16) | ('I' << 24);
        private const int ChunkId_RGBA = 'R' | ('G' << 8) | ('B' << 16) | ('A' << 24);

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
                throw new InvalidDataException("Data is not a valid MagicaVoxel .vox file.");

            int position = 0;

            int mainChildrenEnd = ReadHeaderAndMainChunk(data, ref position);

            int3 modelSize;
            List<(byte x, byte y, byte z, byte colorIndex)> voxels;
            Color32[] palette;
            ReadModelAndPalette(
                data,
                ref position,
                mainChildrenEnd,
                out modelSize,
                out voxels,
                out palette);

            return BuildChunk(modelSize, voxels, palette);
        }

        private int ReadHeaderAndMainChunk(ReadOnlySpan<byte> data, ref int position)
        {
            data.Skip(ref position, FileSignature.Length + sizeof(int));

            int mainId = data.ReadInt32(ref position);
            if (mainId != ChunkId_MAIN)
                throw new InvalidDataException("Invalid .vox file: MAIN chunk not found.");

            int mainContentSize = data.ReadInt32(ref position);
            int mainChildrenSize = data.ReadInt32(ref position);

            data.Skip(ref position, mainContentSize);

            return position + mainChildrenSize;
        }

        private void ReadModelAndPalette(
            ReadOnlySpan<byte> data,
            ref int position,
            int mainChildrenEnd,
            out int3 modelSize,
            out List<(byte x, byte y, byte z, byte colorIndex)> voxels,
            out Color32[] palette)
        {
            int modelIndex = -1;
            modelSize = default;
            bool modelSizeSet = false;

            voxels = new List<(byte x, byte y, byte z, byte colorIndex)>();
            palette = null;

            while (position < mainChildrenEnd)
            {
                if (position + 12 > data.Length)
                    throw new InvalidDataException("Unexpected end of file while reading chunk header.");

                int chunkId = data.ReadInt32(ref position);
                int chunkContentSize = data.ReadInt32(ref position);
                int chunkChildrenSize = data.ReadInt32(ref position);

                int chunkContentStart = position;

                switch (chunkId)
                {
                    case ChunkId_PACK:
                        data.Skip(ref position, chunkContentSize);
                        break;

                    case ChunkId_SIZE:
                        HandleSizeChunk(data, ref position, chunkContentSize,
                            ref modelIndex, ref modelSize, ref modelSizeSet);
                        break;

                    case ChunkId_XYZI:
                        HandleXyziChunk(data, ref position, chunkContentSize,
                            modelIndex, voxels);
                        break;

                    case ChunkId_RGBA:
                        palette = data.ReadVoxRgbaPalette(ref position);
                        break;

                    default:
                        data.Skip(ref position, chunkContentSize);
                        break;
                }

                AlignToChunkEnd(data, ref position, chunkContentStart, chunkContentSize);

                if (chunkChildrenSize > 0)
                {
                    data.Skip(ref position, chunkChildrenSize);
                }
            }

            if (!modelSizeSet)
                throw new InvalidDataException("Invalid .vox file: SIZE chunk for first model not found.");

            if (voxels.Count == 0)
                throw new InvalidDataException("Invalid .vox file: XYZI chunk for first model not found or empty.");
        }

        private void HandleSizeChunk(
            ReadOnlySpan<byte> data,
            ref int position,
            int chunkContentSize,
            ref int modelIndex,
            ref int3 modelSize,
            ref bool modelSizeSet)
        {
            modelIndex++;

            if (modelIndex == 0)
            {
                modelSize = data.ReadVoxSize(ref position);
                modelSizeSet = true;
            }
            else
            {
                data.Skip(ref position, chunkContentSize);
            }
        }

        private void HandleXyziChunk(
            ReadOnlySpan<byte> data,
            ref int position,
            int chunkContentSize,
            int modelIndex,
            List<(byte x, byte y, byte z, byte colorIndex)> voxels)
        {
            if (modelIndex == 0)
            {
                data.ReadVoxXyzi(ref position, voxels);
            }
            else
            {
                data.Skip(ref position, chunkContentSize);
            }
        }

        private static void AlignToChunkEnd(
            ReadOnlySpan<byte> data,
            ref int position,
            int chunkContentStart,
            int chunkContentSize)
        {
            int consumed = position - chunkContentStart;
            if (consumed < chunkContentSize)
            {
                data.Skip(ref position, chunkContentSize - consumed);
            }
            else if (consumed > chunkContentSize)
            {
                throw new InvalidDataException("Chunk content over-read; file is likely corrupted.");
            }
        }

        private VoxelChunk BuildChunk(
            int3 modelSize,
            List<(byte x, byte y, byte z, byte colorIndex)> voxels,
            Color32[] palette)
        {
            bool hasColors = palette != null;

            int3 unityModelSize = new int3(modelSize.x, modelSize.z, modelSize.y);

            VoxelChunk chunk = new VoxelChunk(unityModelSize, hasColors);

            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<Color32> colorArray = hasColors ? chunk.Colors : default;

            int voxelCount = unityModelSize.x * unityModelSize.y * unityModelSize.z;


            foreach (var voxel in voxels)
            {
                // Convert MagicaVoxel axes (Z-up) to Unity axes (Y-up).
                int x = voxel.x;
                int y = voxel.z;
                int z = voxel.y;

                if (x >= unityModelSize.x || y >= unityModelSize.y || z >= unityModelSize.z)
                {
                    continue;
                }

                int index = x + unityModelSize.x * (y + unityModelSize.y * z);
                if (index < 0 || index >= voxelCount)
                    continue;

                voxelTypes[index] = VoxelType.Solid;

                if (hasColors)
                {
                    if (voxel.colorIndex == 0) continue;

                    colorArray[index] = palette[voxel.colorIndex - 1];
                }
            }

            return chunk;
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
            return version == SupportedVersion;
        }
    }
}
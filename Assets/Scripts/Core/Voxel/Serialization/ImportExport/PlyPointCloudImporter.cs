using LedenevTV.Extensions.Span;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace LedenevTV.Voxel.Serialization
{
    public sealed class PlyPointCloudImporter : IChunkImporter
    {
        private bool _useFixedChunkSize = false;
        private int3 _fixedChunkSize;

        private const string PlySignature = "ply";

        private static readonly string[] HeaderPrefixLines =
        {
            "ply",
            "format ascii 1.0",
            "comment : MagicaVoxel @ Ephtracy"
        };

        private static readonly string[] PropertyLines =
        {
            "property float x",
            "property float y",
            "property float z",
            "property uchar red",
            "property uchar green",
            "property uchar blue"
        };

        private const string ElementVertexPrefix = "element vertex";
        private const string EndHeaderLine = "end_header";

        public bool CanImport(ReadOnlySpan<byte> data)
        {
            if (!HasValidSignature(data))
                return false;

            return TryReadHeader(data, out _, out _);
        }

        public VoxelChunk FromBytes(ReadOnlySpan<byte> data)
        {
            if (!TryReadHeader(data, out int vertexCount, out int headerEndOffset))
                throw new InvalidDataException("Data is not a valid MagicaVoxel ASCII PLY.");

            if (vertexCount <= 0)
                throw new InvalidDataException("PLY file has no vertices.");

            List<int3> positions = new List<int3>(vertexCount);
            List<Color32> colors = new List<Color32>(vertexCount);

            ReadVertices(data, headerEndOffset, vertexCount, positions, colors, out int3 min, out int3 max);

            int3 chunkSize = GetChunkSize(min, max);

            return BuildChunk(chunkSize, positions, colors, min);
        }

        public void EnableFixedSize(int3 size)
        {
            _fixedChunkSize = size;
            _useFixedChunkSize = true;
        }

        public void DisableFixedSize()
        {
            _useFixedChunkSize = false;
        }

        private static bool HasValidSignature(ReadOnlySpan<byte> data)
        {
            if (data.Length < PlySignature.Length)
                return false;

            ReadOnlySpan<byte> head = data.Slice(0, PlySignature.Length);
            return head.EqualsAscii(PlySignature);
        }

        private static bool TryReadHeader(ReadOnlySpan<byte> data, out int vertexCount, out int headerEndOffset)
        {
            vertexCount = 0;
            headerEndOffset = 0;

            int position = 0;

            for (int i = 0; i < HeaderPrefixLines.Length; i++)
            {
                ReadOnlySpan<byte> line = data.ReadLine(ref position).TrimAscii();

                if (!line.EqualsAscii(HeaderPrefixLines[i]))
                    return false;
            }
            {
                ReadOnlySpan<byte> line = data.ReadLine(ref position).TrimAscii();

                if (!line.StartsWithAscii(ElementVertexPrefix))
                    return false;

                ReadOnlySpan<byte> countSpan = line.Slice(ElementVertexPrefix.Length).TrimAscii();

                if (!countSpan.TryParseIntAscii(out vertexCount) || vertexCount <= 0)
                    return false;
            }

            for (int i = 0; i < PropertyLines.Length; i++)
            {
                ReadOnlySpan<byte> line = data.ReadLine(ref position).TrimAscii();

                if (!line.EqualsAscii(PropertyLines[i]))
                    return false;
            }
            {
                ReadOnlySpan<byte> line = data.ReadLine(ref position).TrimAscii();

                if (!line.EqualsAscii(EndHeaderLine))
                    return false;
            }

            headerEndOffset = position;
            return true;
        }

        private static void ReadVertices(ReadOnlySpan<byte> data,
                int startOffset,
                int vertexCount,
                List<int3> positions,
                List<Color32> colors,
                out int3 min,
                out int3 max)
        {
            int position = startOffset;

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;

            for (int i = 0; i < vertexCount; i++)
            {
                ReadOnlySpan<byte> line = data.ReadLine(ref position);

                if (line.IsEmptyOrWhiteSpaceAscii())
                    throw new InvalidDataException($"Unexpected empty line at vertex index {i}.");

                int tokenOffset = 0;

                if (!line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> xToken) ||
                    !line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> yToken) ||
                    !line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> zToken) ||
                    !line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> redToken) ||
                    !line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> greenToken) ||
                    !line.TryGetNextTokenAscii(ref tokenOffset, out ReadOnlySpan<byte> blueToken))
                {
                    throw new InvalidDataException($"Invalid vertex format at index {i}.");
                }

                if (!xToken.TryParseIntAscii(out int x) ||
                    !yToken.TryParseIntAscii(out int y) ||
                    !zToken.TryParseIntAscii(out int z))
                {
                    throw new InvalidDataException($"Invalid vertex position at index {i}.");
                }

                if (!redToken.TryParseByteAscii(out byte red) ||
                    !greenToken.TryParseByteAscii(out byte green) ||
                    !blueToken.TryParseByteAscii(out byte blue))
                {
                    throw new InvalidDataException($"Invalid vertex color at index {i}.");
                }

                // Convert MagicaVoxel/PLY axes (Z-up) to Unity axes (Y-up).
                (y, z) = (z, y);

                int3 positionInt3 = new int3(x, y, z);
                positions.Add(positionInt3);
                colors.Add(new Color32(red, green, blue, 255));

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (z < minZ) minZ = z;

                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                if (z > maxZ) maxZ = z;
            }

            if (minX == int.MaxValue)
                throw new InvalidDataException("No valid vertices found in PLY file.");

            min = new int3(minX, minY, minZ);
            max = new int3(maxX, maxY, maxZ);
        }


        private int3 GetChunkSize(int3 min, int3 max)
        {
            if (_useFixedChunkSize)
            {
                return _fixedChunkSize;
            }

            return ComputeChunkSizeFromBounds(min, max);
        }

        private static int3 ComputeChunkSizeFromBounds(int3 min, int3 max)
        {
            int sizeX = max.x - min.x + 1;
            int sizeY = max.y - min.y + 1;
            int sizeZ = max.z - min.z + 1;

            if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
                throw new InvalidDataException($"Invalid PLY bounds: min={min}, max={max}");

            return new int3(sizeX, sizeY, sizeZ);
        }


        private VoxelChunk BuildChunk(int3 chunkSize, List<int3> positions, List<Color32> colors, int3 min)
        {
            bool hasColors = true;

            VoxelChunk chunk = new VoxelChunk(chunkSize, hasColors);
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;
            NativeArray<Color32> colorArray = chunk.Colors;

            int voxelCount = chunkSize.x * chunkSize.y * chunkSize.z;

            if (voxelTypes.Length < voxelCount ||
               materialIds.Length < voxelCount ||
               colorArray.Length < voxelCount)
            {
                throw new InvalidDataException("VoxelChunk arrays have unexpected size.");
            }

            for (int i = 0; i < positions.Count; i++)
            {
                int3 p = positions[i];

                int localX = p.x - min.x;
                int localY = p.y - min.y;
                int localZ = p.z - min.z;

                if (localX < 0 ||
                   localY < 0 ||
                   localZ < 0 ||
                   localX >= chunkSize.x ||
                   localY >= chunkSize.y ||
                   localZ >= chunkSize.z)
                {
                    continue;
                }

                int index = localX + chunkSize.x * (localY + chunkSize.y * localZ);
                if (index < 0 || index >= voxelCount)
                    continue;

                voxelTypes[index] = VoxelType.Solid;

                materialIds[index] = 0;
                colorArray[index] = colors[i];
            }

            return chunk;
        }
    }
}
using System;
using System.IO;

namespace LedenevTV.Voxel.Serialization
{
    public interface IChunkImporter
    {
        /// <summary>Returns true if the data looks like a supported format.</summary>
        bool CanImport(ReadOnlySpan<byte> data);

        /// <summary>Parses chunk from bytes.</summary>
        /// <exception cref="InvalidDataException">Unsupported format or malformed data.</exception>
        VoxelChunk FromBytes(ReadOnlySpan<byte> data);
    }
}
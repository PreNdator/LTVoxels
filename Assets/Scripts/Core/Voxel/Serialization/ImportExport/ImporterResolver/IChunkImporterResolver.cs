using System;

namespace LedenevTV.Voxel.Serialization
{
    public interface IChunkImporterResolver
    {
        /// <summary>
        /// Returns an importer that can import the given data, or <c>null</c> if none match.
        /// </summary>
        IChunkImporter Resolve(ReadOnlySpan<byte> data);
    }
}

using System;
using System.IO;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class ChunkImportService : IChunkImportService
    {
        private readonly IChunkImporterResolver _resolver;

        public ChunkImportService(IChunkImporterResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public VoxelChunk Load(IBytesSource bytesSource)
        {
            if (bytesSource == null)
                throw new ArgumentNullException(nameof(bytesSource));

            byte[] data = bytesSource.GetBytes();

            if (data == null || data.Length == 0)
                throw new InvalidDataException("Data is empty.");

            IChunkImporter importer = _resolver.Resolve(data);

            if (importer == null)
                throw new InvalidDataException("No registered chunk importer can import this data.");

            return importer.FromBytes(data);
        }
    }
}
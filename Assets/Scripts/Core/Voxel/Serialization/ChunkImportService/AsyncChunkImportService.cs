
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class AsyncChunkImportService : IAsyncChunkImportService
    {
        private readonly IChunkImporterResolver _resolver;

        public AsyncChunkImportService(IChunkImporterResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public async Task<VoxelChunk> LoadAsync(IAsyncBytesSource bytesSource, CancellationToken ct = default)
        {
            if (bytesSource == null)
                throw new ArgumentNullException(nameof(bytesSource));

            byte[] data = await bytesSource.GetBytesAsync(ct);

            if (data == null || data.Length == 0)
                throw new InvalidDataException("Data is empty.");

            IChunkImporter importer = _resolver.Resolve(data);

            if (importer == null)
                throw new InvalidDataException("No registered chunk importer can import this data.");

            return importer.FromBytes(data);
        }
    }
}
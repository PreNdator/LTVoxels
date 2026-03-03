using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LedenevTV.Voxel.Serialization
{
    public interface IAsyncChunkImportService
    {
        /// <summary>
        /// Loads chunk bytes from <paramref name="asyncByteSource"/> and imports them using a resolved <see cref="IChunkImporter"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncByteSource"/> is null.</exception>
        /// <exception cref="InvalidDataException">Thrown if data is null/empty or no importer can handle the data.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="ct"/>.</exception>
        Task<VoxelChunk> LoadAsync(IAsyncBytesSource asyncByteSource, CancellationToken ct = default);
    }
}
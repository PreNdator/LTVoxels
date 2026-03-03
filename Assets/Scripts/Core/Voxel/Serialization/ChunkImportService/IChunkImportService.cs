using System;
using System.IO;

namespace LedenevTV.Voxel.Serialization
{

    public interface IChunkImportService
    {
        /// <summary>
        /// Loads chunk bytes from <paramref name="bytesSource"/> and imports them using a resolved <see cref="IChunkImporter"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytesSource"/> is null.</exception>
        /// <exception cref="InvalidDataException">Thrown if data is null/empty or no importer can handle the data.</exception>
        VoxelChunk Load(IBytesSource bytesSource);
    }
}
using System;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class SingleImporter : IChunkImporterResolver
    {
        private readonly IChunkImporter _importer;

        public SingleImporter(IChunkImporter importer)
        {
            if (importer == null) throw new ArgumentNullException(nameof(importer));

            _importer = importer;
        }

        public IChunkImporter Resolve(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                return null;

            if (_importer.CanImport(data))
                return _importer;

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class ChunkImporterResolver : IChunkImporterResolver
    {
        private readonly IChunkImporter[] _importers;

        public ChunkImporterResolver(IEnumerable<IChunkImporter> importers)
        {
            if (importers == null) throw new ArgumentNullException(nameof(importers));

            _importers = importers.Where(i => i != null).ToArray();
        }
        public ChunkImporterResolver(params IChunkImporter[] importers)
        : this((IEnumerable<IChunkImporter>)importers) { }

        public IChunkImporter Resolve(ReadOnlySpan<byte> data)
        {
            if (_importers.Length == 0 || data.IsEmpty)
                return null;

            for (int i = 0; i < _importers.Length; i++)
            {
                IChunkImporter importer = _importers[i];
                if (importer.CanImport(data))
                    return importer;
            }

            return null;
        }
    }
}

using LedenevTV.UnityBridge;
using LedenevTV.Voxel.Drawing;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    /// <summary>
    /// Lazily loads chunks from sources and caches the loaded chunk and its generated mesh.
    /// </summary>
    public sealed class LazyChunkProvider : IChunkProvider, IDisposable
    {
        private readonly IChunkImportService _chunkLoader;
        private readonly IVoxelMeshBuilder _voxelMeshBuilder;
        private readonly IUnityObjectDestroyer _destroyer;

        private readonly Dictionary<IBytesSource, ChunkInfo> _chunkCache = new Dictionary<IBytesSource, ChunkInfo>();

        public LazyChunkProvider(IChunkImportService chunkLoader, IVoxelMeshBuilder voxelMeshBuilder, IUnityObjectDestroyer destroyer)
        {
            _chunkLoader = chunkLoader;
            _voxelMeshBuilder = voxelMeshBuilder;
            _destroyer = destroyer;
        }

        public void Dispose()
        {
            foreach (var info in _chunkCache.Values)
            {
                if (info.SharedMesh != null)
                    _destroyer.Destroy(info.SharedMesh);

                if (info.LoadedChunk != null && info.LoadedChunk.IsAllocated)
                    info.LoadedChunk.Dispose();
            }

            _chunkCache.Clear();
        }

        public VoxelChunk GetChunkClone(IBytesSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            VoxelChunk chunk = GetChunk(source);
            return chunk.Clone();
        }

        public Mesh GetCachedChunkMesh(IBytesSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            VoxelChunk chunk = GetChunk(source);

            ChunkInfo info = _chunkCache[source];

            if (info.SharedMesh == null)
            {
                info.SharedMesh = new Mesh();
                _voxelMeshBuilder.RebuildMesh(info.SharedMesh, chunk, drawFacesOnBounds: true);
            }

            return info.SharedMesh;
        }

        private VoxelChunk GetChunk(IBytesSource source)
        {
            if (_chunkCache.TryGetValue(source, out var info))
                return info.LoadedChunk;

            var chunk = _chunkLoader.Load(source);

            if (info == null)
            {
                info = new ChunkInfo();
                _chunkCache[source] = info;
            }

            info.LoadedChunk = chunk;
            return chunk;
        }

        private sealed class ChunkInfo
        {
            public Mesh SharedMesh;
            public VoxelChunk LoadedChunk;
        }
    }
}
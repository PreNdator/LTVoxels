
using LedenevTV.UnityBridge;
using LedenevTV.Voxel.Drawing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    /// <summary>
    /// Lazily loads chunks from asynchronous sources and caches the loaded chunk and its generated mesh.
    /// </summary>
    public sealed class AsyncLazyChunkProvider : IAsyncChunkProvider, IDisposable
    {
        private IAsyncChunkImportService _chunkImportService;
        private IVoxelMeshBuilder _voxelMeshBuilder;
        private IUnityObjectDestroyer _destroyer;

        private Dictionary<IAsyncBytesSource, ChunkInfo> _chunkCache = new Dictionary<IAsyncBytesSource, ChunkInfo>();

        public AsyncLazyChunkProvider(IAsyncChunkImportService chunkImportService, IVoxelMeshBuilder voxelMeshBuilder, IUnityObjectDestroyer destroyer)
        {
            _chunkImportService = chunkImportService;
            _voxelMeshBuilder = voxelMeshBuilder;
            _destroyer = destroyer;
        }

        public void Dispose()
        {
            foreach (var info in _chunkCache.Values)
            {
                if (info.Cts != null)
                {
                    info.Cts.Cancel();
                    info.Cts.Dispose();
                }

                if (info.SharedMesh != null) _destroyer.Destroy(info.SharedMesh);
                if (info.LoadedChunk != null && info.LoadedChunk.IsAllocated) info.LoadedChunk.Dispose();
            }

            _chunkCache.Clear();
        }

        public async Task<VoxelChunk> GetChunkCloneAsync(IAsyncBytesSource source, CancellationToken ct = default)
        {
            VoxelChunk chunk = await GetChunk(source);
            ct.ThrowIfCancellationRequested();
            return chunk.Clone();
        }

        public async Task<Mesh> GetCachedChunkMeshAsync(IAsyncBytesSource source)
        {
            VoxelChunk chunk = await GetChunk(source);
            if (_chunkCache[source].SharedMesh == null)
            {
                _chunkCache[source].SharedMesh = new Mesh();
                _voxelMeshBuilder.RebuildMesh(_chunkCache[source].SharedMesh, chunk, drawFacesOnBounds: true);
            }
            return _chunkCache[source].SharedMesh;
        }

        private async Task<VoxelChunk> GetChunk(IAsyncBytesSource source)
        {
            if (!_chunkCache.TryGetValue(source, out var info))
            {
                info = new ChunkInfo();
                info.Cts = new CancellationTokenSource();
                info.Task = _chunkImportService.LoadAsync(source, info.Cts.Token);
                _chunkCache[source] = info;
            }
            VoxelChunk chunk = await info.Task;
            info.LoadedChunk = chunk;
            return chunk;
        }

        private class ChunkInfo
        {
            public Mesh SharedMesh;
            public VoxelChunk LoadedChunk;
            public Task<VoxelChunk> Task;
            public CancellationTokenSource Cts;
        }
    }
}
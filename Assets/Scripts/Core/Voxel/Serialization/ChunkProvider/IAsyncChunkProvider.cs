using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    public interface IAsyncChunkProvider
    {
        /// <summary>
        /// Loads the chunk from <paramref name="asyncSource"/> and returns a deep copy of the loaded data.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncSource"/> is null.</exception>
        Task<VoxelChunk> GetChunkCloneAsync(IAsyncBytesSource asyncSource, CancellationToken ct);

        /// <summary>
        /// Loads (or builds) and returns a cached mesh for the chunk stored in <paramref name="asyncSource"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncSource"/> is null.</exception>
        Task<Mesh> GetCachedChunkMeshAsync(IAsyncBytesSource asyncSource);
    }
}
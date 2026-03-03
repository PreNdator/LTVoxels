using System;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    public interface IChunkProvider
    {
        /// <summary>
        /// Loads the chunk from <paramref name="source"/> and returns a deep copy of the loaded data.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        VoxelChunk GetChunkClone(IBytesSource source);

        /// <summary>
        /// Loads (or builds) and returns a cached mesh for the chunk stored in <paramref name="source"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        Mesh GetCachedChunkMesh(IBytesSource source);
    }
}
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Drawing
{
    public interface IChunkSpace
    {
        /// <summary>
        /// Adjusts vertex positions (e.g. shift to recenter the mesh/pivot).
        /// </summary>
        void RepositionVertices(NativeArray<float3> vertices, VoxelChunk voxelChunk);

        /// <summary>
        /// Computes mesh bounds that match the vertex coordinate space.
        /// </summary>
        Bounds GetBounds(VoxelChunk chunk);

        /// <summary>
        /// Returns the chunk's pivot in local voxel coordinates.
        /// </summary>
        float3 GetPivot(VoxelChunk chunk);
    }
}



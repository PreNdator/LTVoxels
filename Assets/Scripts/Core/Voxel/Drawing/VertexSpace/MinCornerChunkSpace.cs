using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Drawing
{
    /// <summary>
    /// Chunk space where the mesh pivot is at the minimum corner (0,0,0).
    /// Vertices are not shifted; bounds are computed in the original chunk space.
    /// </summary>
    public sealed class MinCornerChunkSpace : IChunkSpace
    {
        public void RepositionVertices(NativeArray<float3> vertices, VoxelChunk voxelChunk) { }

        public Bounds GetBounds(VoxelChunk chunk)
        {
            Vector3 size = chunk.SizeV3Int;
            Vector3 center = size * 0.5f;
            return new Bounds(center, size);
        }

        public float3 GetPivot(VoxelChunk chunk)
        {
            return float3.zero;
        }
    }
}



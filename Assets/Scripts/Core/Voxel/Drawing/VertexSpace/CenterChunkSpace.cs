using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Drawing
{
    /// <summary>
    /// Chunk space where the mesh pivot is at the geometric center of the chunk.
    /// Vertices are shifted so that the center ends up at the origin.
    /// </summary>
    public sealed class CenterChunkSpace : IChunkSpace
    {
        public void RepositionVertices(NativeArray<float3> vertices, VoxelChunk voxelChunk)
        {
            float3 center = (float3)voxelChunk.Size * 0.5f;

            ShiftVerticesJob job = new ShiftVerticesJob
            {
                Vertices = vertices,
                Offset = -center
            };

            job.Schedule(vertices.Length, voxelChunk.BatchSize).Complete();
        }

        public Bounds GetBounds(VoxelChunk chunk)
        {
            Vector3 size = chunk.SizeV3Int;
            return new Bounds(Vector3.zero, size);
        }

        public float3 GetPivot(VoxelChunk chunk)
        {
            return (float3)chunk.Size * 0.5f;
        }
    }
}



using LedenevTV.Voxel;
using Unity.Mathematics;
using UnityEngine;


namespace LedenevTV.UnityBridge
{
    public interface IVoxelWorldMapper
    {
        /// <summary>
        /// Converts a world-space position into voxel coordinates inside the given chunk.
        /// Returns false if the position lies outside the chunk bounds.
        /// </summary>
        bool TryWorldToCoord(Vector3 worldPosition, VoxelChunk chunk, Transform chunkTransform, out int3 coord);

        /// <summary>
        /// Converts a world-space position into a linear voxel index inside the given chunk.
        /// Returns false if the position lies outside the chunk bounds.
        /// </summary>
        bool TryWorldToIndex(Vector3 worldPosition, VoxelChunk chunk, Transform chunkTransform, out int index);

        /// <summary>
        /// Converts voxel coordinates into a world-space position at the center of that voxel.
        /// </summary>
        Vector3 CoordToWorld(int3 coord, VoxelChunk chunk, Transform chunkTransform);

        /// <summary>
        /// Converts a linear voxel index into a world-space position at the center of that voxel.
        /// </summary>
        Vector3 IndexToWorld(int index, VoxelChunk chunk, Transform chunkTransform);
    }
}
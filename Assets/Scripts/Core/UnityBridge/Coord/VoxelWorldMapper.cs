using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using System;
using Unity.Mathematics;
using UnityEngine;


namespace LedenevTV.UnityBridge
{
    public sealed class VoxelWorldMapper : IVoxelWorldMapper
    {
        private readonly IChunkSpace _chunkSpace;

        public VoxelWorldMapper(IChunkSpace chunkSpace)
        {
            _chunkSpace = chunkSpace ?? throw new ArgumentNullException(nameof(chunkSpace));
        }

        public bool TryWorldToCoord(Vector3 worldPosition, VoxelChunk chunk, Transform chunkTransform, out int3 coord)
        {
            float3 local = chunkTransform.InverseTransformPoint(worldPosition);

            float3 chunkSpacePos = local + _chunkSpace.GetPivot(chunk);

            coord = (int3)math.floor(chunkSpacePos);

            if (!IsInside(coord, chunk.Size))
            {
                coord = default;
                return false;
            }

            return true;
        }

        public bool TryWorldToIndex(Vector3 worldPosition, VoxelChunk chunk, Transform chunkTransform, out int index)
        {
            index = -1;

            if (!TryWorldToCoord(worldPosition, chunk, chunkTransform, out int3 coord))
                return false;

            index = chunk.CoordToIndex(coord);
            return true;
        }

        public Vector3 CoordToWorld(int3 coord, VoxelChunk chunk, Transform chunkTransform)
        {
            if (!IsInside(coord, chunk.Size))
                throw new ArgumentOutOfRangeException(nameof(coord));

            float3 chunkSpacePos = (float3)coord + new float3(0.5f, 0.5f, 0.5f);

            float3 local = chunkSpacePos - _chunkSpace.GetPivot(chunk);

            return chunkTransform.TransformPoint((Vector3)local);
        }

        public Vector3 IndexToWorld(int index, VoxelChunk chunk, Transform chunkTransform)
        {
            if (!chunk.IsValidIndex(index))
                throw new ArgumentOutOfRangeException(nameof(index));

            int3 coord = chunk.IndexToCoord(index);
            return CoordToWorld(coord, chunk, chunkTransform);
        }

        private static bool IsInside(int3 coord, int3 size)
        {
            return coord.x >= 0 && coord.x < size.x &&
                   coord.y >= 0 && coord.y < size.y &&
                   coord.z >= 0 && coord.z < size.z;
        }
    }
}
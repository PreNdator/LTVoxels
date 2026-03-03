using LedenevTV.Voxel.Drawing;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Splitting
{
    /// <summary>
    /// Splits a chunk into connected components using the provided neighbor offsets.
    /// </summary>
    public class ConnectedComponentsChunkSplitter : IChunkSplitter
    {
        private readonly IReadOnlyList<int3> _offsets;
        private readonly IChunkSpace _chunkSpace;

        public ConnectedComponentsChunkSplitter(INeighborVoxels neighbourVoxels, IChunkSpace chunkSpace)
        {
            _offsets = neighbourVoxels.GetNeighborOffsets();
            _chunkSpace = chunkSpace;
        }


        public List<ChunkPiece> Split(VoxelChunk chunk)
        {
            List<ChunkPiece> pieces = new List<ChunkPiece>();

            int voxelCount = chunk.VoxelsCount;

            NativeArray<byte> visited = new NativeArray<byte>(voxelCount, Allocator.Temp, NativeArrayOptions.ClearMemory);

            NativeList<int> componentIndices = new NativeList<int>(voxelCount, Allocator.Temp);

            NativeRingQueue<int> queue = new NativeRingQueue<int>(voxelCount, Allocator.Temp);

            try
            {
                for (int index = 0; index < voxelCount; index++)
                {
                    if (visited[index] != 0)
                        continue;

                    visited[index] = 1;

                    VoxelType voxelType = chunk.VoxelTypes[index];

                    if (!IsSolid(voxelType))
                        continue;

                    int3 min;
                    int3 max;

                    CollectComponent(chunk, index, visited, componentIndices, queue, out min, out max);

                    ChunkPiece piece = CreateChunkPiece(chunk, componentIndices, min, max);
                    pieces.Add(piece);
                }
            }
            finally
            {
                if (visited.IsCreated)
                    visited.Dispose();

                if (componentIndices.IsCreated)
                    componentIndices.Dispose();

                if (queue.IsCreated)
                    queue.Dispose();
            }

            return pieces;
        }

        private void CollectComponent(
                VoxelChunk chunk,
                int startIndex,
                NativeArray<byte> visited,
                NativeList<int> componentIndices,
                NativeRingQueue<int> queue,
                out int3 min,
                out int3 max)
        {
            int3 size = chunk.Size;

            componentIndices.Clear();

            min = chunk.IndexToCoord(startIndex);
            max = min;

            queue.Enqueue(startIndex);

            while (!queue.IsEmpty)
            {
                int current = queue.Dequeue();
                componentIndices.Add(current);

                int3 coord = chunk.IndexToCoord(current);

                if (coord.x < min.x) min.x = coord.x;
                if (coord.y < min.y) min.y = coord.y;
                if (coord.z < min.z) min.z = coord.z;

                if (coord.x > max.x) max.x = coord.x;
                if (coord.y > max.y) max.y = coord.y;
                if (coord.z > max.z) max.z = coord.z;

                for (int i = 0; i < _offsets.Count; i++)
                {
                    int3 offset = _offsets[i];

                    int3 neighborCoord = coord + offset;

                    if (neighborCoord.x < 0 || neighborCoord.x >= size.x ||
                        neighborCoord.y < 0 || neighborCoord.y >= size.y ||
                        neighborCoord.z < 0 || neighborCoord.z >= size.z)
                    {
                        continue;
                    }

                    int neighbourIndex = chunk.CoordToIndex(
                        neighborCoord.x,
                        neighborCoord.y,
                        neighborCoord.z);

                    if (visited[neighbourIndex] != 0)
                        continue;

                    VoxelType neighborType = chunk.VoxelTypes[neighbourIndex];
                    if (!IsSolid(neighborType))
                        continue;

                    visited[neighbourIndex] = 1;
                    queue.Enqueue(neighbourIndex);
                }
            }
        }


        private ChunkPiece CreateChunkPiece(VoxelChunk sourceChunk, NativeList<int> componentIndices, int3 min, int3 max)
        {
            int3 pieceSize = (max - min) + new int3(1, 1, 1);
            VoxelChunk pieceChunk = new VoxelChunk(pieceSize, sourceChunk.HasColors);

            NativeArray<VoxelType> voxelTypes = sourceChunk.VoxelTypes;
            NativeArray<byte> materialIds = sourceChunk.MaterialIds;
            NativeArray<Color32> colors = sourceChunk.Colors;

            for (int i = 0; i < componentIndices.Length; i++)
            {
                int srcIndex = componentIndices[i];
                int3 srcCoord = sourceChunk.IndexToCoord(srcIndex);

                Vector3Int dstCoord = new Vector3Int(
                    srcCoord.x - min.x,
                    srcCoord.y - min.y,
                    srcCoord.z - min.z);

                VoxelType type = voxelTypes[srcIndex];
                byte material = materialIds[srcIndex];

                if (sourceChunk.HasColors)
                {
                    Color32 color = colors[srcIndex];
                    pieceChunk.TrySetVoxel(dstCoord, type, material, color);
                }
                else
                {
                    pieceChunk.TrySetVoxel(dstCoord, type, material);
                }
            }

            float3 sourcePivot = _chunkSpace.GetPivot(sourceChunk);
            float3 piecePivot = _chunkSpace.GetPivot(pieceChunk);
            Vector3 offset = (Vector3)((float3)min + piecePivot - sourcePivot);

            ChunkPiece piece;
            piece.Chunk = pieceChunk;
            piece.Offset = offset;

            return piece;
        }

        protected virtual bool IsSolid(VoxelType type)
        {
            return type == VoxelType.Solid || type == VoxelType.Transparent;
        }
    }

}

using UnityEngine;

namespace LedenevTV.Voxel.Splitting
{
    public struct ChunkPiece
    {
        /// <summary>
        /// Extracted sub-chunk data for this piece; must be disposed by the caller when no longer needed.
        /// </summary>
        public VoxelChunk Chunk;
        /// <summary>
        /// Local-space offset of this piece relative to the source chunk (in voxel units, where 1 voxel = 1 Unity unit).
        /// </summary>
        public Vector3 Offset;
    }
}

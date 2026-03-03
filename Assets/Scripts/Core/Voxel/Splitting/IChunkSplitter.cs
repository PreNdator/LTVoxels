using System.Collections.Generic;

namespace LedenevTV.Voxel.Splitting
{
    public interface IChunkSplitter
    {
        /// <summary>
        /// Splits <paramref name="voxelChunk"/> into independent pieces.
        /// </summary>
        List<ChunkPiece> Split(VoxelChunk voxelChunk);
    }

}

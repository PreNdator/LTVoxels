using UnityEngine;

namespace LedenevTV.Voxel.Drawing
{
    /// <summary>
    /// Builds a Unity <see cref="Mesh"/> from voxel chunk data.
    /// </summary>
    public interface IVoxelMeshBuilder
    {
        /// <summary>
        /// Rebuilds <paramref name="mesh"/> from <paramref name="voxelChunk"/> data.
        /// </summary>
        /// <param name="mesh">Target mesh to write to.</param>
        /// <param name="voxelChunk">Source voxel chunk.</param>
        /// <param name="drawFacesOnBounds">
        /// If true, faces on the chunk boundary are generated as if outside voxels were empty.
        /// </param>
        public Mesh RebuildMesh(Mesh mesh, VoxelChunk voxelChunk, bool drawFacesOnBounds);
    }
}

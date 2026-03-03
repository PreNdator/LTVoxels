using System.Collections.Generic;
using Unity.Mathematics;

namespace LedenevTV.Voxel
{
    /// <summary>
    /// Provides neighbor voxel offsets for a particular 3D connectivity rule.
    /// </summary>
    public interface INeighborVoxels
    {
        /// <summary>
        /// Returns relative offsets to all neighbor voxels.
        /// </summary>
        IReadOnlyList<int3> GetNeighborOffsets();
    }
}

using System.Collections.Generic;
using Unity.Mathematics;

namespace LedenevTV.Voxel
{
    /// <summary>
    /// 6-connected neighborhood (face-adjacent neighbors only).
    /// </summary>
    public sealed class NeighborVoxels6 : INeighborVoxels
    {
        public static readonly int3[] NeighborOffsets6 =
        {
            new int3( 1,  0,  0),
            new int3(-1,  0,  0),

            new int3( 0,  1,  0),
            new int3( 0, -1,  0),

            new int3( 0,  0,  1),
            new int3( 0,  0, -1),
        };

        public IReadOnlyList<int3> GetNeighborOffsets() => NeighborOffsets6;
    }

}

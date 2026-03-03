using System.Collections.Generic;
using Unity.Mathematics;

namespace LedenevTV.Voxel
{
    /// <summary>
    /// 18-connected neighborhood (face- and edge-adjacent neighbors).
    /// </summary>
    public sealed class NeighborVoxels18 : INeighborVoxels
    {
        public static readonly int3[] NeighborOffsets18 =
        {
            new int3( 1,  0,  0),
            new int3(-1,  0,  0),

            new int3( 0,  1,  0),
            new int3( 0, -1,  0),

            new int3( 0,  0,  1),
            new int3( 0,  0, -1),

            new int3( 1,  1,  0),
            new int3( 1, -1,  0),
            new int3(-1,  1,  0),
            new int3(-1, -1,  0),

            new int3( 1,  0,  1),
            new int3( 1,  0, -1),
            new int3(-1,  0,  1),
            new int3(-1,  0, -1),

            new int3( 0,  1,  1),
            new int3( 0,  1, -1),
            new int3( 0, -1,  1),
            new int3( 0, -1, -1),
        };

        public IReadOnlyList<int3> GetNeighborOffsets() => NeighborOffsets18;
    }

}

using System.Collections.Generic;
using Unity.Mathematics;

namespace LedenevTV.Voxel
{
    /// <summary>
    /// 26-connected neighborhood (face-, edge-, and corner-adjacent neighbors).
    /// </summary>
    public sealed class NeighborVoxels26 : INeighborVoxels
    {
        public static readonly int3[] NeighborOffsets26 =
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

            new int3( 1,  1,  1),
            new int3( 1,  1, -1),
            new int3( 1, -1,  1),
            new int3( 1, -1, -1),
            new int3(-1,  1,  1),
            new int3(-1,  1, -1),
            new int3(-1, -1,  1),
            new int3(-1, -1, -1),
        };

        public IReadOnlyList<int3> GetNeighborOffsets() => NeighborOffsets26;
    }

}

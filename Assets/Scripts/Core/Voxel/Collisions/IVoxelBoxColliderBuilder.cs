using System.Collections.Generic;

namespace LedenevTV.Voxel.Collisions
{
    public interface IVoxelBoxColliderBuilder
    {
        void Build(VoxelChunk voxelChunk, List<VoxelBoxColliderData> results);
    }
}

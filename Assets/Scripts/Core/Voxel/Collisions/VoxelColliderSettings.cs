namespace LedenevTV.Voxel.Collisions
{
    public sealed class VoxelColliderSettings
    {
        public bool IncludeTransparentVoxels { get; }

        public VoxelColliderSettings(bool includeTransparentVoxels = true)
        {
            IncludeTransparentVoxels = includeTransparentVoxels;
        }

        public bool IsCollidable(VoxelType type)
        {
            if (type == VoxelType.Solid)
                return true;

            return IncludeTransparentVoxels && type == VoxelType.Transparent;
        }
    }
}

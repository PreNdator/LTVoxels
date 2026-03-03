namespace LedenevTV.Voxel.Drawing
{
    internal static class VoxelFaceMask
    {
        public const byte PosX = 1 << 0;
        public const byte NegX = 1 << 1;
        public const byte PosY = 1 << 2;
        public const byte NegY = 1 << 3;
        public const byte PosZ = 1 << 4;
        public const byte NegZ = 1 << 5;
    }
}
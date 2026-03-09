namespace LedenevTV
{
    public sealed class VoxelChunkPathAttribute : StreamingAssetsPathAttribute
    {
        public VoxelChunkPathAttribute() : base(".ply", ".vox", ".bytes", ".txt", ".bin") { }
    }
}
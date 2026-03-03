namespace LedenevTV.Voxel.Drawing
{
    public sealed class VoxelMeshSettings
    {
        public int MaterialLimit { get; }

        public VoxelMeshSettings(byte materialLimit)
        {
            if (materialLimit == 0)
                throw new System.ArgumentOutOfRangeException(nameof(materialLimit));

            MaterialLimit = materialLimit;
        }
    }
}



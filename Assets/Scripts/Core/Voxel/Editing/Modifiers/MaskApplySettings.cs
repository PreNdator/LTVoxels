using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Settings that control how a mask is applied to a voxel chunk.
    /// </summary>
    public struct MaskApplySettings
    {
        /// <summary>Enables applying <see cref="VoxType"/>.</summary>
        public bool IsModifyVoxelType;

        /// <summary>Enables applying <see cref="MaterialId"/>.</summary>
        public bool IsModifyMaterialId;

        /// <summary>Enables applying <see cref="Color"/>.</summary>
        public bool IsModifyColors;

        /// <summary>Target color used when <see cref="IsModifyColors"/> is true.</summary>
        public Color32 Color;

        /// <summary>Target voxel type used when <see cref="IsModifyVoxelType"/> is true.</summary>
        public VoxelType VoxType;

        /// <summary>Target material Id used when <see cref="IsModifyMaterialId"/> is true.</summary>
        public byte MaterialId;
    }
}


using UnityEngine;

namespace LedenevTV.Voxel.Collisions
{
    public readonly struct VoxelBoxColliderData
    {
        public readonly Vector3 Center;
        public readonly Vector3 Size;

        public VoxelBoxColliderData(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }
    }
}

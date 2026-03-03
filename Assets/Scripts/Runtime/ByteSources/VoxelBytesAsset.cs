using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    public sealed class VoxelBytesAsset : ScriptableObject, IBytesSource
    {
        [SerializeField] private byte[] _data;
        public byte[] Data => _data;

        public byte[] GetBytes()
        {
            return _data;
        }

#if UNITY_EDITOR
        public void SetData(byte[] data) => _data = data;
#endif
    }
}
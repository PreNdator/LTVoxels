using Unity.Collections;
using Unity.Mathematics;


namespace LedenevTV.Voxel
{
    public class DamagableChunk: VoxelChunk
    {
        private NativeArray<float> _damageMultiplier;
        public NativeArray<float> DamageMultiplier => _damageMultiplier;

        protected override void Allocate(int3 size, bool useColors)
        {
            base.Allocate(size, useColors);

            int voxelCount = Size.x * Size.y * Size.z;
            _damageMultiplier = new NativeArray<float>(voxelCount, Allocator.Persistent);
        }

        protected override void DisposeNativeArrays()
        {
            base.DisposeNativeArrays();
            if (_damageMultiplier.IsCreated) _damageMultiplier.Dispose();
        }
    }
}

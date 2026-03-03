using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace LedenevTV.Voxel.Editing
{
    [BurstCompile]
    internal struct FillAllChunkJob : IJobParallelFor
    {
        public NativeArray<int> Mask;

        public void Execute(int index)
        {
            Mask[index] = index;
        }
    }
}


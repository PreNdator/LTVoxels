using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace LedenevTV.Voxel.Drawing
{
    [BurstCompile]
    internal struct ShiftVerticesJob : IJobParallelFor
    {
        public NativeArray<float3> Vertices;
        public float3 Offset;

        public void Execute(int index)
        {
            Vertices[index] += Offset;
        }
    }
}



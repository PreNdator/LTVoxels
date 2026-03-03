using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace LedenevTV.Voxel.Editing
{
    [BurstCompile]
    internal struct SphereMaskJob : IJobParallelFor
    {
        public NativeArray<byte> Mask;

        public int3 ChunkSize;
        public int3 Center;
        public float RadiusSq;

        public void Execute(int index)
        {
            int3 size = ChunkIndexing.IndexToCoord(index, ChunkSize);


            float dx = size.x - Center.x;
            float dy = size.y - Center.y;
            float dz = size.z - Center.z;

            Mask[index] = (dx * dx + dy * dy + dz * dz <= RadiusSq) ? (byte)1 : (byte)0;
        }
    }
}


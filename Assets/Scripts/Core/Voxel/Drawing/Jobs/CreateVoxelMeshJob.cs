using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace LedenevTV.Voxel.Drawing
{
    [BurstCompile]
    internal struct CreateVoxelMeshJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<byte> MaterialId;
        [ReadOnly]
        public NativeArray<byte> VisibleFacesMask;
        [ReadOnly]
        public NativeArray<int> PerFaceOffset;
        [ReadOnly]
        public NativeArray<int> PerMaterialOffset;
        [ReadOnly]
        public int3 ChunkSize;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float3> Vertices;
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float3> Normals;
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float2> UVs;
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<uint> Indices;

        private static readonly float3 NormalPosX = new float3(1f, 0f, 0f);
        private static readonly float3 NormalNegX = new float3(-1f, 0f, 0f);
        private static readonly float3 NormalPosY = new float3(0f, 1f, 0f);
        private static readonly float3 NormalNegY = new float3(0f, -1f, 0f);
        private static readonly float3 NormalPosZ = new float3(0f, 0f, 1f);
        private static readonly float3 NormalNegZ = new float3(0f, 0f, -1f);

        private static readonly float2 UV00 = new float2(0f, 0f);
        private static readonly float2 UV10 = new float2(1f, 0f);
        private static readonly float2 UV01 = new float2(0f, 1f);
        private static readonly float2 UV11 = new float2(1f, 1f);

        public void Execute(int index)
        {
            byte mask = VisibleFacesMask[index];
            if (mask == 0)
                return;

            int3 size = ChunkIndexing.IndexToCoord(index, ChunkSize);

            int material = MaterialId[index];
            int faceBase = PerMaterialOffset[material] + PerFaceOffset[index];

            int vertexOffset = faceBase * VoxelMeshConstants.VerticesPerFace;
            int indexOffset = faceBase * VoxelMeshConstants.IndicesPerFace;

            int posX = size.x + 1;
            int posY = size.y + 1;
            int posZ = size.z + 1;

            float3 v000 = new float3(size.x, size.y, size.z);
            float3 v100 = new float3(posX, size.y, size.z);
            float3 v010 = new float3(size.x, posY, size.z);
            float3 v110 = new float3(posX, posY, size.z);
            float3 v001 = new float3(size.x, size.y, posZ);
            float3 v101 = new float3(posX, size.y, posZ);
            float3 v011 = new float3(size.x, posY, posZ);
            float3 v111 = new float3(posX, posY, posZ);

            if ((mask & VoxelFaceMask.PosX) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v100, v110, v101, v111, NormalPosX);
            }

            if ((mask & VoxelFaceMask.NegX) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v000, v001, v010, v011, NormalNegX);
            }

            if ((mask & VoxelFaceMask.PosY) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v010, v011, v110, v111, NormalPosY);
            }

            if ((mask & VoxelFaceMask.NegY) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v000, v100, v001, v101, NormalNegY);
            }

            if ((mask & VoxelFaceMask.PosZ) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v001, v101, v011, v111, NormalPosZ);
            }

            if ((mask & VoxelFaceMask.NegZ) != 0)
            {
                WriteFace(ref vertexOffset, ref indexOffset, v000, v010, v100, v110, NormalNegZ);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteFace(
           ref int vertexOffset,
           ref int indexOffset,
           float3 v0, float3 v1, float3 v2, float3 v3,
           float3 normal)
        {
            Vertices[vertexOffset + 0] = v0;
            Vertices[vertexOffset + 1] = v1;
            Vertices[vertexOffset + 2] = v2;
            Vertices[vertexOffset + 3] = v3;

            Normals[vertexOffset + 0] = normal;
            Normals[vertexOffset + 1] = normal;
            Normals[vertexOffset + 2] = normal;
            Normals[vertexOffset + 3] = normal;

            UVs[vertexOffset + 0] = UV00;
            UVs[vertexOffset + 1] = UV10;
            UVs[vertexOffset + 2] = UV01;
            UVs[vertexOffset + 3] = UV11;

            Indices[indexOffset + 0] = (uint)(vertexOffset + 0);
            Indices[indexOffset + 1] = (uint)(vertexOffset + 1);
            Indices[indexOffset + 2] = (uint)(vertexOffset + 2);
            Indices[indexOffset + 3] = (uint)(vertexOffset + 3);
            Indices[indexOffset + 4] = (uint)(vertexOffset + 2);
            Indices[indexOffset + 5] = (uint)(vertexOffset + 1);

            vertexOffset += VoxelMeshConstants.VerticesPerFace;
            indexOffset += VoxelMeshConstants.IndicesPerFace;
        }
    }
}
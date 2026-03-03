using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LedenevTV.Voxel.Drawing
{
    [BurstCompile]
    internal struct ColorVerticesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> Colors;
        [ReadOnly]
        public NativeArray<byte> PerVoxelCount;
        [ReadOnly]
        public NativeArray<int> PerVoxelOffset;
        [ReadOnly]
        public NativeArray<int> PerMaterialOffset;
        [ReadOnly]
        public NativeArray<byte> MaterialId;

        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<Color32> MeshColors;

        public void Execute(int index)
        {
            byte facesCount = PerVoxelCount[index];
            if (facesCount == 0)
            {
                return;
            }

            int mat = MaterialId[index];
            int materialFaceOffset = PerMaterialOffset[mat];
            int voxelFaceOffset = PerVoxelOffset[index];

            int globalFaceOffset = materialFaceOffset + voxelFaceOffset;

            int vertexStart = globalFaceOffset * VoxelMeshConstants.VerticesPerFace;
            int verticesToColor = facesCount * VoxelMeshConstants.VerticesPerFace;

            Color32 color = Colors[index];

            for (int i = 0; i < verticesToColor; ++i)
            {
                MeshColors[vertexStart + i] = color;
            }
        }
    }
}
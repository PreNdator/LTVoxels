using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace LedenevTV.Voxel.Drawing
{

    /// <summary>
    /// Builds a cube-based voxel mesh with axis-aligned faces using Unity Jobs.
    /// </summary>
    public class VoxelMeshBuilder : IVoxelMeshBuilder
    {
        private int _materialLimit;

        private IChunkSpace _vertexPostProcessor;

        public VoxelMeshBuilder(VoxelMeshSettings buildSettings, IChunkSpace vertexPostProcessor)
        {
            _vertexPostProcessor = vertexPostProcessor;
            _materialLimit = buildSettings.MaterialLimit;
        }

        public Mesh RebuildMesh(Mesh mesh, VoxelChunk voxelChunk, bool drawFacesOnBounds)
        {
            NativeArray<byte> visibleFacesMask = default;
            NativeArray<byte> visibleFacesCount = default;
            NativeArray<int> perMaterialCount = default;
            NativeArray<int> perMaterialOffset = default;
            NativeArray<int> perVoxelFaceOffset = default;

            try
            {
                CalculateVisibleFaces(
                    voxelChunk,
                    drawFacesOnBounds,
                    out visibleFacesMask,
                    out visibleFacesCount
                );

                int facesCount = CountFaces(
                    visibleFacesCount,
                    voxelChunk.MaterialIds,
                    out perMaterialCount
                );

                CalculatePerMaterialsOffset(perMaterialCount, out perMaterialOffset);
                CalculateOffset(visibleFacesCount, voxelChunk.MaterialIds, out perVoxelFaceOffset);

                CreateMesh(
                    mesh,
                    voxelChunk,
                    facesCount,
                    visibleFacesMask,
                    perVoxelFaceOffset,
                    perMaterialOffset,
                    visibleFacesCount
                );
            }
            finally
            {
                if (perVoxelFaceOffset.IsCreated) perVoxelFaceOffset.Dispose();
                if (perMaterialOffset.IsCreated) perMaterialOffset.Dispose();
                if (perMaterialCount.IsCreated) perMaterialCount.Dispose();
                if (visibleFacesMask.IsCreated) visibleFacesMask.Dispose();
                if (visibleFacesCount.IsCreated) visibleFacesCount.Dispose();
            }

            return mesh;
        }

        internal void CreateMesh(
            Mesh mesh,
            VoxelChunk voxelChunk,
            int facesCount,
            NativeArray<byte> visibleFacesMask,
            NativeArray<int> perVoxelOffset,
            NativeArray<int> perMaterialOffset,
            NativeArray<byte> perVoxelCount)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            int vertexCount = facesCount * VoxelMeshConstants.VerticesPerFace;
            int indexCount = facesCount * VoxelMeshConstants.IndicesPerFace;

            bool useColors = voxelChunk.HasColors;

            VertexAttributeDescriptor[] vertexAttributes = BuildVertexAttributes(useColors);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            NativeArray<float3> vertices = meshData.GetVertexData<float3>(0);
            NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
            NativeArray<float2> uvs = meshData.GetVertexData<float2>(2);
            NativeArray<uint> indices = meshData.GetIndexData<uint>();

            CreateVoxelMeshJob createVoxelMesh = new CreateVoxelMeshJob
            {
                MaterialId = voxelChunk.MaterialIds,
                VisibleFacesMask = visibleFacesMask,
                PerFaceOffset = perVoxelOffset,
                PerMaterialOffset = perMaterialOffset,
                ChunkSize = voxelChunk.Size,

                Vertices = vertices,
                Normals = normals,
                UVs = uvs,
                Indices = indices
            };

            JobHandle createHandle = createVoxelMesh.Schedule(voxelChunk.VoxelTypes.Length, voxelChunk.BatchSize);

            JobHandle finalHandle = createHandle;

            if (useColors)
            {
                NativeArray<Color32> meshColors = meshData.GetVertexData<Color32>(3);

                ColorVerticesJob colorVerticies = new ColorVerticesJob
                {
                    Colors = voxelChunk.Colors,
                    PerVoxelCount = perVoxelCount,
                    PerMaterialOffset = perMaterialOffset,
                    PerVoxelOffset = perVoxelOffset,
                    MaterialId = voxelChunk.MaterialIds,

                    MeshColors = meshColors
                };

                finalHandle = colorVerticies.Schedule(voxelChunk.VoxelTypes.Length, voxelChunk.BatchSize, createHandle);
            }

            finalHandle.Complete();

            _vertexPostProcessor.RepositionVertices(vertices, voxelChunk);

            CreateSubmeshes(meshData, perMaterialOffset, indices);

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

            mesh.bounds = _vertexPostProcessor.GetBounds(voxelChunk);

        }
        internal VertexAttributeDescriptor[] BuildVertexAttributes(bool useColors)
        {
            int attributeCount = useColors ? 4 : 3;
            VertexAttributeDescriptor[] vertexAttributes = new VertexAttributeDescriptor[attributeCount];

            vertexAttributes[0] = new VertexAttributeDescriptor(
                VertexAttribute.Position,
                VertexAttributeFormat.Float32,
                3,
                0
            );

            vertexAttributes[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal,
                VertexAttributeFormat.Float32,
                3,
                1
            );

            vertexAttributes[2] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32,
                2,
                2
            );

            if (useColors)
            {
                vertexAttributes[3] = new VertexAttributeDescriptor(
                    VertexAttribute.Color,
                    VertexAttributeFormat.UNorm8,
                    4,
                    3
                );
            }

            return vertexAttributes;
        }

        internal void CreateSubmeshes(Mesh.MeshData meshData, NativeArray<int> perMaterialOffset, NativeArray<uint> indices)
        {
            int subMeshCount = perMaterialOffset.Length;
            meshData.subMeshCount = subMeshCount;

            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                int startIndex = perMaterialOffset[subMeshIndex] * VoxelMeshConstants.IndicesPerFace;

                int endIndex;
                if (subMeshIndex == subMeshCount - 1)
                {
                    endIndex = indices.Length;
                }
                else
                {
                    endIndex = perMaterialOffset[subMeshIndex + 1] * VoxelMeshConstants.IndicesPerFace;
                }

                int indexLength = endIndex - startIndex;

                SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(startIndex, indexLength);
                subMeshDescriptor.topology = MeshTopology.Triangles;

                meshData.SetSubMesh(
                    subMeshIndex,
                    subMeshDescriptor,
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds |
                    MeshUpdateFlags.DontValidateIndices
                );
            }
        }

        internal void CalculateOffset(NativeArray<byte> visibleFacesCount, NativeArray<byte> materialId, out NativeArray<int> offset)
        {
            int length = visibleFacesCount.Length;

            int materialCount = _materialLimit;

            offset = new NativeArray<int>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            NativeArray<int> runningPerMaterial = new NativeArray<int>(materialCount, Allocator.Temp, NativeArrayOptions.ClearMemory);

            try
            {
                for (int i = 0; i < length; ++i)
                {
                    byte faces = visibleFacesCount[i];
                    if (faces == 0)
                    {
                        // For voxels with no visible faces, the offset value doesn't matter since it will never be used.
                        continue;
                    }


                    int currMaterial = materialId[i];

                    int currentOffset = runningPerMaterial[currMaterial];
                    offset[i] = currentOffset;
                    runningPerMaterial[currMaterial] = currentOffset + faces;
                }
            }
            finally
            {
                runningPerMaterial.Dispose();
            }
        }

        internal void CalculatePerMaterialsOffset(NativeArray<int> perMaterialCount, out NativeArray<int> perMaterialsOffset)
        {

            perMaterialsOffset = new NativeArray<int>(_materialLimit, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            perMaterialsOffset[0] = 0;

            for (int i = 1; i < _materialLimit; ++i)
            {
                perMaterialsOffset[i] = perMaterialCount[i - 1] + perMaterialsOffset[i - 1];
            }
        }

        internal int CountFaces(NativeArray<byte> visibleFacesCount, NativeArray<byte> materialId, out NativeArray<int> perMaterialCount)
        {
            int length = visibleFacesCount.Length;

            perMaterialCount = new NativeArray<int>(_materialLimit, Allocator.TempJob, NativeArrayOptions.ClearMemory);

            int count = 0;

            for (int i = 0; i < length; ++i)
            {
                count += visibleFacesCount[i];

                int currMaterialId = materialId[i];

                perMaterialCount[currMaterialId] += visibleFacesCount[i];
            }
            return count;
        }

        internal void CalculateVisibleFaces(VoxelChunk voxelChunk, bool drawFacesOnBounds, out NativeArray<byte> visibleFacesMask, out NativeArray<byte> visibleFacesCount)
        {
            int length = voxelChunk.VoxelTypes.Length;

            visibleFacesMask = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            visibleFacesCount = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            VoxelType outVoxel = drawFacesOnBounds ? VoxelType.Empty : VoxelType.Outside;

            CalculateVisibleFacesJob calculateVisibleFacesJob = new CalculateVisibleFacesJob
            {
                VoxelTypes = voxelChunk.VoxelTypes,
                ChunkSize = voxelChunk.Size,

                VisibleFacesMask = visibleFacesMask,
                VisibleFacesCount = visibleFacesCount,
                OutOfBoundsVoxel = outVoxel
            };

            JobHandle calculateVisibleFacesHandle = calculateVisibleFacesJob.Schedule(length, voxelChunk.BatchSize);

            calculateVisibleFacesHandle.Complete();
        }
    }
}



using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LedenevTV.Tests.Voxel
{
    public class VoxelMeshBuilderTests
    {
        private GameObject _meshObject;
        private Mesh _mesh;
        private IChunkSpace _vertexPostProcessor;
        private VoxelMeshSettings _meshSettings;

        [SetUp]
        public void SetUp()
        {
            _meshObject = new GameObject("VoxelMeshTest");
            _mesh = new Mesh();
            _vertexPostProcessor = new MinCornerChunkSpace();
            _meshSettings = new VoxelMeshSettings(1);
        }

        [TearDown]
        public void TearDown()
        {
            if (_mesh != null)
            {
                Object.DestroyImmediate(_mesh);
            }

            if (_meshObject != null)
            {
                Object.DestroyImmediate(_meshObject);
            }
        }

        private static void SetVoxel(VoxelChunk chunk, int index, VoxelType type, byte materialId)
        {
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;
            NativeArray<byte> materialIds = chunk.MaterialIds;

            voxelTypes[index] = type;
            materialIds[index] = materialId;
        }

        private static void SetColor(VoxelChunk chunk, int index, Color32 color)
        {
            NativeArray<Color32> colors = chunk.Colors;
            colors[index] = color;
        }

        [Test]
        public void Builder_VertexAttributes_WithoutColors_NoColorAttribute()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            VertexAttributeDescriptor[] attrs = builder.BuildVertexAttributes(useColors: false);

            Assert.AreEqual(3, attrs.Length);
            Assert.AreEqual(VertexAttribute.Position, attrs[0].attribute);
            Assert.AreEqual(VertexAttribute.Normal, attrs[1].attribute);
            Assert.AreEqual(VertexAttribute.TexCoord0, attrs[2].attribute);
        }

        [Test]
        public void Builder_VertexAttributes_WithColors_HasColorAttribute()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            VertexAttributeDescriptor[] attrs = builder.BuildVertexAttributes(useColors: true);

            Assert.AreEqual(4, attrs.Length);
            Assert.AreEqual(VertexAttribute.Color, attrs[3].attribute);
        }

        [Test]
        public void RebuildMesh_WithoutChunkColors_MeshHasNoColorStream()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 3, useColors: false))
            {
                int index = ChunkIndexing.CoordToIndex(1, 1, 1, chunk.Size);

                SetVoxel(chunk, index, VoxelType.Solid, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(24, _mesh.vertexCount);
                Assert.IsFalse(_mesh.HasVertexAttribute(VertexAttribute.Color));
                Assert.AreEqual(0, _mesh.colors32.Length);
            }
        }

        [Test]
        public void RebuildMesh_WithChunkColors_MeshHasColorStream_AndColorsApplied()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 3, useColors: true))
            {
                int index = ChunkIndexing.CoordToIndex(1, 1, 1, chunk.Size);

                SetVoxel(chunk, index, VoxelType.Solid, 0);

                Color32 expected = new Color32(10, 20, 30, 255);
                SetColor(chunk, index, expected);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(24, _mesh.vertexCount);
                Assert.IsTrue(_mesh.HasVertexAttribute(VertexAttribute.Color));

                Color32[] colors = _mesh.colors32;
                Assert.AreEqual(_mesh.vertexCount, colors.Length);

                for (int k = 0; k < colors.Length; k++)
                {
                    Assert.AreEqual(expected, colors[k]);
                }
            }
        }

        [Test]
        public void OneSolidCube_InCenter_Has6Faces_24Verts_36Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 3, useColors: false))
            {
                int index = ChunkIndexing.CoordToIndex(1, 1, 1, chunk.Size);

                SetVoxel(chunk, index, VoxelType.Solid, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(24, _mesh.vertexCount);
                Assert.AreEqual(36, (int)_mesh.GetIndexCount(0));
            }
        }

        [Test]
        public void ThreeSolidCubes_InLine_Has14Faces_56Verts_84Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 5, useColors: false))
            {
                int y = 2, z = 2;

                int i1 = ChunkIndexing.CoordToIndex(1, y, z, chunk.Size);
                int i2 = ChunkIndexing.CoordToIndex(2, y, z, chunk.Size);
                int i3 = ChunkIndexing.CoordToIndex(3, y, z, chunk.Size);

                SetVoxel(chunk, i1, VoxelType.Solid, 0);
                SetVoxel(chunk, i2, VoxelType.Solid, 0);
                SetVoxel(chunk, i3, VoxelType.Solid, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(56, _mesh.vertexCount);
                Assert.AreEqual(84, (int)_mesh.GetIndexCount(0));
            }
        }

        [Test]
        public void SolidNextToTransparent_Has11Faces_44Verts_66Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 5, useColors: false))
            {
                int y = 2, z = 2;

                int solidIndex = ChunkIndexing.CoordToIndex(2, y, z, chunk.Size);
                int transparentIndex = ChunkIndexing.CoordToIndex(3, y, z, chunk.Size);

                SetVoxel(chunk, solidIndex, VoxelType.Solid, 0);
                SetVoxel(chunk, transparentIndex, VoxelType.Transparent, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(44, _mesh.vertexCount);
                Assert.AreEqual(66, (int)_mesh.GetIndexCount(0));
            }
        }

        [Test]
        public void TwoTransparentAdjacent_Has10Faces_40Verts_60Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 5, useColors: false))
            {
                int y = 2, z = 2;

                int a = ChunkIndexing.CoordToIndex(2, y, z, chunk.Size);
                int b = ChunkIndexing.CoordToIndex(3, y, z, chunk.Size);

                SetVoxel(chunk, a, VoxelType.Transparent, 0);
                SetVoxel(chunk, b, VoxelType.Transparent, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(40, _mesh.vertexCount);
                Assert.AreEqual(60, (int)_mesh.GetIndexCount(0));
            }
        }

        [Test]
        public void TwoIsolatedSolidCubes_DifferentMaterials_SubmeshIndexCountsMatch()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(new VoxelMeshSettings(2), _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 5, useColors: false))
            {
                int a = ChunkIndexing.CoordToIndex(1, 2, 2, chunk.Size);
                int b = ChunkIndexing.CoordToIndex(3, 2, 2, chunk.Size);

                SetVoxel(chunk, a, VoxelType.Solid, 0);
                SetVoxel(chunk, b, VoxelType.Solid, 1);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(2, _mesh.subMeshCount);
                Assert.AreEqual(48, _mesh.vertexCount);

                Assert.AreEqual(36, (int)_mesh.GetIndexCount(0));
                Assert.AreEqual(36, (int)_mesh.GetIndexCount(1));
            }
        }

        [Test]
        public void Offsets_PerMaterial_DoNotOverlap()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(new VoxelMeshSettings(3), _vertexPostProcessor);

            NativeArray<byte> visible = new NativeArray<byte>(6, Allocator.TempJob);
            NativeArray<byte> mat = new NativeArray<byte>(6, Allocator.TempJob);

            visible[0] = 2; mat[0] = 0;
            visible[1] = 0; mat[1] = 0;
            visible[2] = 1; mat[2] = 1;
            visible[3] = 3; mat[3] = 0;
            visible[4] = 2; mat[4] = 2;
            visible[5] = 1; mat[5] = 2;

            NativeArray<int> perMatCount = default;
            NativeArray<int> perMatOffset = default;
            NativeArray<int> perVoxelOffset = default;

            try
            {
                int totalFaces = builder.CountFaces(visible, mat, out perMatCount);
                Assert.AreEqual(9, totalFaces);

                builder.CalculatePerMaterialsOffset(perMatCount, out perMatOffset);
                builder.CalculateOffset(visible, mat, out perVoxelOffset);

                for (int m = 0; m < perMatOffset.Length; m++)
                {
                    int rangeStart = perMatOffset[m];
                    int rangeEnd = rangeStart + perMatCount[m];

                    int lastEnd = rangeStart;

                    for (int i = 0; i < visible.Length; i++)
                    {
                        if (visible[i] == 0)
                            continue;
                        if (mat[i] != m)
                            continue;

                        int start = perMatOffset[m] + perVoxelOffset[i];
                        int end = start + visible[i];

                        Assert.GreaterOrEqual(start, rangeStart);
                        Assert.LessOrEqual(end, rangeEnd);

                        Assert.GreaterOrEqual(start, lastEnd, $"Overlap in material {m} at voxel {i}");
                        lastEnd = Mathf.Max(lastEnd, end);
                    }
                }
            }
            finally
            {
                visible.Dispose();
                mat.Dispose();

                if (perMatCount.IsCreated) perMatCount.Dispose();
                if (perMatOffset.IsCreated) perMatOffset.Dispose();
                if (perVoxelOffset.IsCreated) perVoxelOffset.Dispose();
            }
        }

        [Test]
        public void SolidCube_InCorner_CurrentBehavior_Has3Faces_12Verts_18Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(new VoxelMeshSettings(3), _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 3, useColors: false))
            {
                int index = ChunkIndexing.CoordToIndex(0, 0, 0, chunk.Size);

                SetVoxel(chunk, index, VoxelType.Solid, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: false);

                Assert.AreEqual(12, _mesh.vertexCount);
                Assert.AreEqual(18, (int)_mesh.GetIndexCount(0));
            }
        }

        [Test]
        public void SolidCube_InCorner_DrawFacesOnBoundsTrue_Has6Faces_24Verts_36Indices()
        {
            VoxelMeshBuilder builder = new VoxelMeshBuilder(_meshSettings, _vertexPostProcessor);

            using (VoxelChunk chunk = new VoxelChunk(size: 3, useColors: false))
            {
                int index = ChunkIndexing.CoordToIndex(0, 0, 0, chunk.Size);

                SetVoxel(chunk, index, VoxelType.Solid, 0);

                builder.RebuildMesh(_mesh, chunk, drawFacesOnBounds: true);

                Assert.AreEqual(24, _mesh.vertexCount);
                Assert.AreEqual(36, (int)_mesh.GetIndexCount(0));
            }
        }
    }
}
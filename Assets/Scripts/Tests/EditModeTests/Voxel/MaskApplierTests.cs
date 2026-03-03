using LedenevTV.Voxel;
using LedenevTV.Voxel.Editing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public class MaskApplierTests
    {
        [Test]
        public void VoxelTypeMaskApplier_OneVoxel_ChangesOnlyThatIndex()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                VoxelTypeMaskApplier applier = new VoxelTypeMaskApplier();
                VoxelType targetType = (VoxelType)2;

                NativeArray<VoxelType> expectedVoxelTypes = expected.VoxelTypes;
                expectedVoxelTypes[idx] = targetType;

                applier.Apply(chunk, mask, targetType);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void MaterialMaskApplier_OneVoxel_ChangesOnlyThatIndex()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                MaterialMaskApplier applier = new MaterialMaskApplier();
                byte targetMat = 123;

                NativeArray<byte> expectedMaterialIds = expected.MaterialIds;
                expectedMaterialIds[idx] = targetMat;

                applier.Apply(chunk, mask, targetMat);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ColorMaskApplier_OneVoxel_ChangesOnlyThatIndex_WhenChunkHasColors()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                ColorMaskApplier applier = new ColorMaskApplier();
                Color32 targetColor = new Color32(10, 20, 30, 40);

                NativeArray<Color32> expectedColors = expected.Colors;
                expectedColors[idx] = targetColor;

                applier.Apply(chunk, mask, targetColor);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ColorMaskApplier_DoesNothing_WhenChunkHasNoColors()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: false, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                ColorMaskApplier applier = new ColorMaskApplier();

                applier.Apply(chunk, mask, new Color32(1, 2, 3, 4));

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ChunkMaskApplier_ModifyAll_OneVoxel_ChangesVoxelTypeMaterialAndColor()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                ChunkMaskApplier applier = new ChunkMaskApplier();

                MaskApplySettings settings = new MaskApplySettings
                {
                    IsModifyVoxelType = true,
                    VoxType = (VoxelType)1,
                    IsModifyMaterialId = true,
                    MaterialId = 200,
                    IsModifyColors = true,
                    Color = new Color32(5, 6, 7, 8)
                };

                NativeArray<VoxelType> expectedVoxelTypes = expected.VoxelTypes;
                NativeArray<byte> expectedMaterialIds = expected.MaterialIds;
                NativeArray<Color32> expectedColors = expected.Colors;

                expectedVoxelTypes[idx] = settings.VoxType;
                expectedMaterialIds[idx] = settings.MaterialId;
                expectedColors[idx] = settings.Color;

                applier.Apply(chunk, mask, settings);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ChunkMaskApplier_ModifyColors_FlagTrue_ButChunkHasNoColors_DoesNothing()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: false, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx = 0;
                OneVoxelMaskCreator mask = new OneVoxelMaskCreator(idx);

                ChunkMaskApplier applier = new ChunkMaskApplier();

                MaskApplySettings settings = new MaskApplySettings
                {
                    IsModifyVoxelType = false,
                    IsModifyMaterialId = false,
                    IsModifyColors = true,
                    Color = new Color32(9, 9, 9, 9)
                };

                applier.Apply(chunk, mask, settings);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ChunkMaskApplier_NoFlags_DoesNothing()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                ChunkMaskApplier applier = new ChunkMaskApplier();

                MaskApplySettings settings = new MaskApplySettings
                {
                    IsModifyVoxelType = false,
                    IsModifyMaterialId = false,
                    IsModifyColors = false
                };

                applier.Apply(chunk, new OneVoxelMaskCreator(0), settings);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void ChunkMaskApplier_CombineMask_ChangesTwoIndices_LeavesOthersIntact()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                int idx0 = 0;
                int idx1 = math.min(1, chunk.VoxelsCount - 1);

                OneVoxelMaskCreator mask0 = new OneVoxelMaskCreator(idx0);
                OneVoxelMaskCreator mask1 = new OneVoxelMaskCreator(idx1);
                CombineMaskCreator combined = new CombineMaskCreator(mask0, mask1);

                ChunkMaskApplier applier = new ChunkMaskApplier();

                MaskApplySettings settings = new MaskApplySettings
                {
                    IsModifyVoxelType = true,
                    VoxType = (VoxelType)2,
                    IsModifyMaterialId = true,
                    MaterialId = 77,
                    IsModifyColors = true,
                    Color = new Color32(100, 101, 102, 103)
                };

                NativeArray<VoxelType> expectedVoxelTypes = expected.VoxelTypes;
                NativeArray<byte> expectedMaterialIds = expected.MaterialIds;
                NativeArray<Color32> expectedColors = expected.Colors;

                expectedVoxelTypes[idx0] = settings.VoxType;
                expectedMaterialIds[idx0] = settings.MaterialId;
                expectedColors[idx0] = settings.Color;

                expectedVoxelTypes[idx1] = settings.VoxType;
                expectedMaterialIds[idx1] = settings.MaterialId;
                expectedColors[idx1] = settings.Color;

                applier.Apply(chunk, combined, settings);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }

        [Test]
        public void AnyApplier_EmptyMask_DoesNothing()
        {
            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true, seed: 321);
            using (chunk)
            using (VoxelChunk expected = chunk.Clone())
            {
                OneVoxelMaskCreator emptyMask = new OneVoxelMaskCreator(chunk.VoxelsCount + 1000);

                new VoxelTypeMaskApplier().Apply(chunk, emptyMask, (VoxelType)1);
                new MaterialMaskApplier().Apply(chunk, emptyMask, 10);
                new ColorMaskApplier().Apply(chunk, emptyMask, new Color32(1, 2, 3, 4));

                MaskApplySettings settings = new MaskApplySettings
                {
                    IsModifyVoxelType = true,
                    VoxType = (VoxelType)1,
                    IsModifyMaterialId = true,
                    MaterialId = 10,
                    IsModifyColors = true,
                    Color = new Color32(1, 2, 3, 4)
                };

                new ChunkMaskApplier().Apply(chunk, emptyMask, settings);

                VoxelChunkTestHelpers.AreEqual(expected, chunk);
            }
        }
    }
}
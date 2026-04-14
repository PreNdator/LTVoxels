using LedenevTV.Voxel;
using LedenevTV.Voxel.Editing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public sealed class ChunkBreakerTests
    {
        [Test]
        public void Break_AppliesDamage_OnlyToMaskedVoxel()
        {
            using DamagableChunk chunk = new DamagableChunk();
            chunk.Rebuild(new int3(3, 1, 1), true);

            NativeArray<float> damageMultiplier = chunk.DamageMultiplier;
            damageMultiplier[0] = 1f;
            damageMultiplier[1] = 1f;
            damageMultiplier[2] = 1f;

            chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));
            chunk.TrySetVoxel(new Vector3Int(1, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));
            chunk.TrySetVoxel(new Vector3Int(2, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));

            ChunkBreaker breaker = new ChunkBreaker();

            using NativeList<int> broken = breaker.Break(chunk, new OneVoxelMaskCreator(1), 10f);

            NativeArray<Color32> colors = chunk.Colors;

            Assert.That(broken.Length, Is.EqualTo(0));
            Assert.That(colors[0].a, Is.EqualTo(100));
            Assert.That(colors[1].a, Is.EqualTo(90));
            Assert.That(colors[2].a, Is.EqualTo(100));
        }

        [Test]
        public void Break_WhenAlphaReachesZero_MakesVoxelEmpty_AndReturnsItsIndex()
        {
            using DamagableChunk chunk = new DamagableChunk();
            chunk.Rebuild(new int3(2, 1, 1), true);

            NativeArray<float> damageMultiplier = chunk.DamageMultiplier;
            damageMultiplier[0] = 1f;
            damageMultiplier[1] = 1f;

            chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 5));
            chunk.TrySetVoxel(new Vector3Int(1, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));

            ChunkBreaker breaker = new ChunkBreaker();

            using NativeList<int> broken = breaker.Break(chunk, new OneVoxelMaskCreator(0), 10f);

            NativeArray<Color32> colors = chunk.Colors;
            NativeArray<VoxelType> voxelTypes = chunk.VoxelTypes;

            Assert.That(broken.Length, Is.EqualTo(1));
            Assert.That(broken[0], Is.EqualTo(0));
            Assert.That(colors[0].a, Is.EqualTo(0));
            Assert.That(voxelTypes[0], Is.EqualTo(VoxelType.Empty));
            Assert.That(voxelTypes[1], Is.EqualTo(VoxelType.Solid));
        }

        [Test]
        public void Break_UsesDamageMultiplier()
        {
            using DamagableChunk chunk = new DamagableChunk();
            chunk.Rebuild(new int3(2, 1, 1), true);

            NativeArray<float> damageMultiplier = chunk.DamageMultiplier;
            damageMultiplier[0] = 1f;
            damageMultiplier[1] = 2f;

            chunk.TrySetVoxel(new Vector3Int(0, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));
            chunk.TrySetVoxel(new Vector3Int(1, 0, 0), VoxelType.Solid, 0, new Color32(0, 0, 0, 100));

            ChunkBreaker breaker = new ChunkBreaker();

            using NativeList<int> broken 
                = breaker.Break(chunk, new CombineMaskCreator(new OneVoxelMaskCreator(0), new OneVoxelMaskCreator(1)), 10f);

            NativeArray<Color32> colors = chunk.Colors;

            Assert.That(broken.Length, Is.EqualTo(0));
            Assert.That(colors[0].a, Is.EqualTo(90));
            Assert.That(colors[1].a, Is.EqualTo(80));
        }
    }
}
using Cysharp.Threading.Tasks;
using LedenevTV.UnityBridge;
using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using LedenevTV.Voxel.Serialization;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Voxel
{
    public class LazyChunkProviderTests
    {
        private Mock<IAsyncChunkImportService> _loaderMock;
        private Mock<IVoxelMeshBuilder> _meshBuilderMock;
        private Mock<IUnityObjectDestroyer> _destroyerMock;

        [SetUp]
        public void SetUp()
        {
            _loaderMock = new Mock<IAsyncChunkImportService>();
            _meshBuilderMock = new Mock<IVoxelMeshBuilder>();
            _destroyerMock = new Mock<IUnityObjectDestroyer>();
        }

        [Test]
        public async Task GetChunkCloneAsync_LoadsChunkAndReturnsDeepClone()
        {
            Mock<IAsyncBytesSource> source = new Mock<IAsyncBytesSource>();

            VoxelChunk originalChunk = VoxelChunkTestHelpers.CreateRandomChunk(size: 4, useColors: true);

            _loaderMock
                .Setup(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalChunk);

            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            VoxelChunk clone = default;

            try
            {
                clone = await provider.GetChunkCloneAsync(source.Object);

                _loaderMock.Verify(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()), Times.Once);

                VoxelChunkTestHelpers.AreEqual(originalChunk, clone);

                if (clone.VoxelsCount > 0)
                {
                    int index = 0;
                    VoxelType originalVoxel = originalChunk.VoxelTypes[index];
                    NativeArray<VoxelType> voxelTypes = clone.VoxelTypes;
                    voxelTypes[index] = (VoxelType)(((int)originalVoxel + 1) % 3);

                    Assert.AreEqual(originalVoxel, originalChunk.VoxelTypes[index], "Original chunk was modified when clone was changed.");
                }
            }
            finally
            {
                if (clone.IsAllocated)
                    clone.Dispose();

                provider.Dispose();
            }
        }

        [Test]
        public async Task GetChunkCloneAsync_UsesCachedChunk_OnSubsequentCalls()
        {
            Mock<IAsyncBytesSource> source = new Mock<IAsyncBytesSource>();

            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(size: 4, useColors: false);

            _loaderMock
                .Setup(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(chunk);

            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            VoxelChunk clone1 = default;
            VoxelChunk clone2 = default;

            try
            {
                clone1 = await provider.GetChunkCloneAsync(source.Object);
                clone2 = await provider.GetChunkCloneAsync(source.Object);

                _loaderMock.Verify(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()), Times.Once);

                VoxelChunkTestHelpers.AreEqual(chunk, clone1);
                VoxelChunkTestHelpers.AreEqual(chunk, clone2);
            }
            finally
            {
                if (clone1.IsAllocated) clone1.Dispose();
                if (clone2.IsAllocated) clone2.Dispose();
                provider.Dispose();
            }
        }

        [Test]
        public async Task GetChunkMeshAsync_BuildsMeshOnceAndReusesIt()
        {
            Mock<IAsyncBytesSource> source = new Mock<IAsyncBytesSource>();

            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(size: 8, useColors: true);

            _loaderMock
                .Setup(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(chunk);

            int rebuildCalls = 0;
            Mesh meshPassedToBuilder = null;

            _meshBuilderMock
                .Setup(b => b.RebuildMesh(It.IsAny<Mesh>(), It.IsAny<VoxelChunk>(), It.IsAny<bool>()))
                .Callback<Mesh, VoxelChunk, bool>((m, c, drawFacesOnBounds) =>
                {
                    rebuildCalls++;
                    meshPassedToBuilder = m;
                    Assert.IsTrue(drawFacesOnBounds, "drawFacesOnBounds should always be true.");
                });

            AsyncLazyChunkProvider provider =
                new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            try
            {
                Mesh mesh1 = await provider.GetCachedChunkMeshAsync(source.Object);
                Mesh mesh2 = await provider.GetCachedChunkMeshAsync(source.Object);

                _loaderMock.Verify(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()), Times.Once);

                Assert.AreEqual(1, rebuildCalls, "Mesh should be rebuilt only once per source.");
                Assert.AreSame(mesh1, mesh2, "Mesh instance must be cached per source.");
                Assert.AreSame(mesh1, meshPassedToBuilder, "Cached mesh must be the same as used in builder.");
            }
            finally
            {
                provider.Dispose();
            }
        }

        [Test]
        public async Task GetChunkMeshAsync_DifferentSourcesHaveDifferentMeshes()
        {
            Mock<IAsyncBytesSource> source1 = new Mock<IAsyncBytesSource>();
            Mock<IAsyncBytesSource> source2 = new Mock<IAsyncBytesSource>();

            VoxelChunk chunk1 = VoxelChunkTestHelpers.CreateRandomChunk(size: 4, useColors: false, seed: 1);
            VoxelChunk chunk2 = VoxelChunkTestHelpers.CreateRandomChunk(size: 4, useColors: false, seed: 2);

            _loaderMock
                .Setup(l => l.LoadAsync(It.IsAny<IAsyncBytesSource>(), It.IsAny<CancellationToken>()))
                .Returns<IAsyncBytesSource, CancellationToken>((s, ct) =>
                {
                    if (ReferenceEquals(s, source1.Object)) return Task.FromResult(chunk1);
                    if (ReferenceEquals(s, source2.Object)) return Task.FromResult(chunk2);
                    throw new ArgumentException("Unexpected source");
                });

            int rebuildCalls = 0;
            _meshBuilderMock
                .Setup(b => b.RebuildMesh(It.IsAny<Mesh>(), It.IsAny<VoxelChunk>(), It.IsAny<bool>()))
                .Callback(() => rebuildCalls++);

            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            try
            {
                Mesh mesh1 = await provider.GetCachedChunkMeshAsync(source1.Object);
                Mesh mesh2 = await provider.GetCachedChunkMeshAsync(source2.Object);

                _loaderMock.Verify(l => l.LoadAsync(It.IsAny<IAsyncBytesSource>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

                Assert.AreEqual(2, rebuildCalls, "Each source should cause its own mesh build.");
                Assert.AreNotSame(mesh1, mesh2, "Different sources must not share same mesh instance.");
            }
            finally
            {
                provider.Dispose();
            }
        }

        [Test]
        public async Task Dispose_DestroysSharedMeshAndDisposesChunk()
        {
            Mock<IAsyncBytesSource> source = new Mock<IAsyncBytesSource>();

            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(4, 4, 4), useColors: true);

            _loaderMock
                .Setup(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(chunk);

            Mesh createdMesh = null;

            _meshBuilderMock
                .Setup(b => b.RebuildMesh(It.IsAny<Mesh>(), It.IsAny<VoxelChunk>(), It.IsAny<bool>()))
                .Callback<Mesh, VoxelChunk, bool>((m, c, _) =>
                {
                    createdMesh = m;
                });

            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            await provider.GetCachedChunkMeshAsync(source.Object);

            provider.Dispose();

            if (createdMesh != null)
            {
                _destroyerMock.Verify(d => d.Destroy(createdMesh), Times.Once, "Destroyer should be called exactly once for cached mesh.");
            }

            Assert.IsFalse(chunk.IsAllocated, "Chunk should be disposed by provider.Dispose().");
        }

        [Test]
        public void Dispose_WhenNoChunksLoaded_DoesNotThrow()
        {
            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            Assert.DoesNotThrow(() => provider.Dispose());
        }

        [Test]
        public async Task GetChunkMeshAsync_ConcurrentCalls_LoadChunkOnceAndShareMesh()
        {
            Mock<IAsyncBytesSource> source = new Mock<IAsyncBytesSource>();

            VoxelChunk chunk = VoxelChunkTestHelpers.CreateRandomChunk(new int3(8, 8, 8), useColors: true);

            TaskCompletionSource<VoxelChunk> tcs = new TaskCompletionSource<VoxelChunk>();

            _loaderMock
                .Setup(l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()))
                .Returns(tcs.Task);

            int rebuildCalls = 0;
            Mesh meshPassedToBuilder = null;

            _meshBuilderMock
                .Setup(b => b.RebuildMesh(It.IsAny<Mesh>(), It.IsAny<VoxelChunk>(), It.IsAny<bool>()))
                .Callback<Mesh, VoxelChunk, bool>((m, c, drawFacesOnBounds) =>
                {
                    rebuildCalls++;
                    meshPassedToBuilder = m;
                    Assert.IsTrue(drawFacesOnBounds, "drawFacesOnBounds should always be true.");
                });

            AsyncLazyChunkProvider provider = new AsyncLazyChunkProvider(_loaderMock.Object, _meshBuilderMock.Object, _destroyerMock.Object);

            try
            {
                Task<Mesh> task1 = provider.GetCachedChunkMeshAsync(source.Object);
                Task<Mesh> task2 = provider.GetCachedChunkMeshAsync(source.Object);

                _loaderMock.Verify(
                    l => l.LoadAsync(source.Object, It.IsAny<CancellationToken>()),
                    Times.Once);

                tcs.SetResult(chunk);

                Mesh[] results = await Task.WhenAll(task1, task2);
                Mesh mesh1 = results[0];
                Mesh mesh2 = results[1];

                Assert.AreEqual(1, rebuildCalls, "Mesh should be rebuilt only once for concurrent calls.");
                Assert.AreSame(mesh1, mesh2, "Concurrent calls must share the same mesh instance.");
                Assert.AreSame(mesh1, meshPassedToBuilder, "Mesh passed to builder must be the cached instance.");
            }
            finally
            {
                provider.Dispose();
            }
        }
    }
}


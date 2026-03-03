
using LedenevTV.Voxel;
using LedenevTV.Voxel.Serialization;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace LedenevTV.Tests.Voxel
{
    public class ChunkImportServiceTests
    {
        [Test]
        public async Task LoadAsync_WithValidData_RestoresChunk()
        {
            int3 size = 5;

            VoxchImportExport chunkImportExport = new VoxchImportExport();
            SingleImporter resolver = new SingleImporter(chunkImportExport);
            AsyncChunkImportService loader = new AsyncChunkImportService(resolver);

            VoxelChunk originalChunk = null;
            VoxelChunk loadedChunk = null;

            try
            {
                originalChunk = VoxelChunkTestHelpers.CreateRandomChunk(
                    size: size,
                    useColors: true,
                    seed: 42,
                    batchSize: 64
                );

                byte[] bytes = chunkImportExport.ToBytes(originalChunk);

                IAsyncBytesSource bytesSource = CreateBytesSource(bytes);
                loadedChunk = await loader.LoadAsync(bytesSource, CancellationToken.None);

                VoxelChunkTestHelpers.AreEqual(originalChunk, loadedChunk);
            }
            finally
            {
                originalChunk?.Dispose();
                loadedChunk?.Dispose();
            }
        }

        private static IAsyncBytesSource CreateBytesSource(byte[] bytes)
        {
            Mock<IAsyncBytesSource> mock = new Mock<IAsyncBytesSource>();
            mock.Setup(x => x.GetBytesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(bytes);
            return mock.Object;
        }
    }
}
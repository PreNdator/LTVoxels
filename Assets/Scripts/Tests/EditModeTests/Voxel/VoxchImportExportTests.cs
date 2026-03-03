using LedenevTV.Voxel;
using LedenevTV.Voxel.Serialization;
using NUnit.Framework;
using System.IO;


namespace LedenevTV.Tests.Voxel
{
    public class VoxchImportExportTests
    {
        private VoxchImportExport _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new VoxchImportExport();
        }

        [Test]
        public void ToBytesAndFromBytes_WithoutColors_RestoresVoxelTypesAndMaterialIds()
        {
            const int size = 4;

            VoxelChunk originalChunk = null;
            VoxelChunk deserializedChunk = null;

            try
            {
                originalChunk = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: false);

                byte[] bytes = _converter.ToBytes(originalChunk);
                deserializedChunk = _converter.FromBytes(bytes);

                VoxelChunkTestHelpers.AreEqual(originalChunk, deserializedChunk);
            }
            finally
            {
                originalChunk?.Dispose();
                deserializedChunk?.Dispose();
            }
        }

        [Test]
        public void ToBytesAndFromBytes_WithColors_RestoresAllData()
        {
            const int size = 3;

            VoxelChunk originalChunk = null;
            VoxelChunk deserializedChunk = null;

            try
            {
                originalChunk = VoxelChunkTestHelpers.CreateRandomChunk(size, useColors: true);

                byte[] bytes = _converter.ToBytes(originalChunk);
                deserializedChunk = _converter.FromBytes(bytes);

                VoxelChunkTestHelpers.AreEqual(originalChunk, deserializedChunk);
            }
            finally
            {
                originalChunk?.Dispose();
                deserializedChunk?.Dispose();
            }
        }

        [Test]
        public void FromBytes_InvalidData_ThrowsInvalidDataException()
        {
            byte[] invalidData = { 1, 2, 3, 4, 5, 6, 7, 8 };

            Assert.Throws<InvalidDataException>(() => _converter.FromBytes(invalidData));

            Assert.Throws<InvalidDataException>(() => _converter.FromBytes(null));
        }
    }
}

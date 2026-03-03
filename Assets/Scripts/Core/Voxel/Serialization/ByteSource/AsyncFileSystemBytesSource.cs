using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace LedenevTV.Voxel.Serialization
{
    public sealed class AsyncFileSystemBytesSource : IAsyncBytesSource
    {
        public string FilePath { get; }

        public AsyncFileSystemBytesSource(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is null or empty.", nameof(filePath));

            FilePath = filePath;
        }

        public Task<byte[]> GetBytesAsync(CancellationToken ct = default)
        {
            return File.ReadAllBytesAsync(FilePath, ct);
        }
    }
}
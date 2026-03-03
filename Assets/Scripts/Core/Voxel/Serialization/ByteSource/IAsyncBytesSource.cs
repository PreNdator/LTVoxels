using System.Threading;
using System.Threading.Tasks;

namespace LedenevTV.Voxel.Serialization
{
    public interface IAsyncBytesSource
    {
        /// <summary>
        /// Returns raw bytes from the specified source asynchronously.
        /// </summary>
        Task<byte[]> GetBytesAsync(CancellationToken ct = default);
    }
}
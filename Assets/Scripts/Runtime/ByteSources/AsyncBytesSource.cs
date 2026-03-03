
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LedenevTV.Voxel.Serialization
{
    public abstract class AsyncBytesSource : ScriptableObject, IAsyncBytesSource
    {
        public abstract Task<byte[]> GetBytesAsync(CancellationToken ct = default);
    }
}
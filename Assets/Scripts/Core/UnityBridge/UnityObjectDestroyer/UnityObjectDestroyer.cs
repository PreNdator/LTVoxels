using UnityEngine;

namespace LedenevTV.UnityBridge
{
    /// <summary>
    /// Destroys Unity objects using <see cref="Object.Destroy(Object)"/>.
    /// </summary>
    public sealed class UnityObjectDestroyer : IUnityObjectDestroyer
    {
        public void Destroy(Object obj)
        {
            if (obj != null)
                Object.Destroy(obj);
        }
    }
}
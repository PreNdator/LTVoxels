using UnityEngine;

namespace LedenevTV.UnityBridge
{
    /// <summary>
    /// Destroys Unity objects using <see cref="Object.DestroyImmediate(Object)"/>.
    /// </summary>
    public sealed class UnityObjectDestroyerImmediate : IUnityObjectDestroyer
    {
        public void Destroy(Object obj)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
    }
}
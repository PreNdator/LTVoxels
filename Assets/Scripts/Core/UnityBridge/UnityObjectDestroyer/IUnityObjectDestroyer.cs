using UnityEngine;

namespace LedenevTV.UnityBridge
{
    /// <summary>
    /// Abstraction for destroying Unity objects.
    /// </summary>
    public interface IUnityObjectDestroyer
    {
        /// <summary>
        /// Destroys the specified Unity object. If <paramref name="obj"/> is null, does nothing.
        /// </summary>
        void Destroy(Object obj);
    }
}
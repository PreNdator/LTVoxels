using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace LedenevTV.Voxel.Collisions
{
    [DisallowMultipleComponent]
    public sealed class VoxelBoxColliderGroup : MonoBehaviour
    {
        [SerializeField, HideInInspector] private List<BoxCollider> _colliders = new List<BoxCollider>();

        private readonly List<VoxelBoxColliderData> _boxes = new List<VoxelBoxColliderData>();
        private IVoxelBoxColliderBuilder _builder;

        public int ActiveColliderCount { get; private set; }

        [Inject]
        public void Construct(IVoxelBoxColliderBuilder builder)
        {
            _builder = builder;
        }

        public void Rebuild(VoxelChunk voxelChunk)
        {
            if (_builder == null)
            {
                Debug.LogWarning("Missing IVoxelBoxColliderBuilder", this);
                Clear();
                return;
            }

            _builder.Build(voxelChunk, _boxes);
            Apply(_boxes);
        }

        public void Clear()
        {
            ActiveColliderCount = 0;

            for (int i = 0; i < _colliders.Count; i++)
            {
                BoxCollider collider = _colliders[i];
                if (collider != null)
                    collider.enabled = false;
            }
        }

        public void SetCollidersEnabled(bool isEnabled)
        {
            for (int i = 0; i < ActiveColliderCount && i < _colliders.Count; i++)
            {
                BoxCollider collider = _colliders[i];
                if (collider != null)
                    collider.enabled = isEnabled;
            }
        }

        private void Apply(List<VoxelBoxColliderData> boxes)
        {
            RemoveMissingColliders();
            EnsureCapacity(boxes.Count);

            for (int i = 0; i < boxes.Count; i++)
            {
                BoxCollider collider = _colliders[i];
                VoxelBoxColliderData box = boxes[i];

                collider.center = box.Center;
                collider.size = box.Size;
                collider.enabled = isActiveAndEnabled;
            }

            ActiveColliderCount = boxes.Count;

            for (int i = ActiveColliderCount; i < _colliders.Count; i++)
            {
                BoxCollider collider = _colliders[i];
                if (collider != null)
                    collider.enabled = false;
            }
        }

        private void EnsureCapacity(int count)
        {
            while (_colliders.Count < count)
            {
                BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                collider.enabled = false;
                _colliders.Add(collider);
            }
        }

        private void RemoveMissingColliders()
        {
            for (int i = _colliders.Count - 1; i >= 0; i--)
            {
                if (_colliders[i] == null)
                    _colliders.RemoveAt(i);
            }
        }
    }
}

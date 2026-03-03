using LedenevTV.Voxel;
using LedenevTV.Voxel.Editing;
using UnityEngine;
using Zenject;


namespace LedenevTV.Runtime.Examples
{
    public class SphereAnimation : ChunkEditAnimation
    {
        [Header("Sphere")]

        [SerializeField]
        private int _chunkSize = 32;

        [SerializeField]
        private float _radius = 2;

        [SerializeField, Range(0.01f, 10f)]
        private float _stepMoveDistance = 1f;

        [Header("Color")]

        [SerializeField]
        private Gradient _trailColor;

        [SerializeField]
        private float _colorChangeSpeed = 1;



        private int _currStep = 0;
        private Vector3 _direction;
        private Vector3 _currPosition;
        private Vector3Int _lastDrawnCenter;

        private IChunkMaskApplier _chunkMaskApplier;

        [Inject]
        private void Construct(IChunkMaskApplier chunkMaskApplier)
        {
            _chunkMaskApplier = chunkMaskApplier;
        }

        public override void ClearAnimationSettings()
        {
            float center = _chunkSize * 0.5f;
            _currPosition = new Vector3(center, center, center);

            _currStep = 0;

            _direction = Random.onUnitSphere;

            _lastDrawnCenter = ToVoxelCenter(_currPosition);
        }

        public override void RebuildChunk(VoxelChunk chunk)
        {
            chunk.Rebuild(_chunkSize, useColors: true);

            Vector3Int center = ToVoxelCenter(_currPosition);
            _lastDrawnCenter = center;
            ApplySphere(chunk, center);
        }

        public override void NextStep(VoxelChunk chunk)
        {
            ++_currStep;

            Vector3Int newCenter;

            float min = _radius;
            float max = (_chunkSize - 1) - _radius;

            MoveAndBounce(min, max);

            newCenter = ToVoxelCenter(_currPosition);

            if (newCenter != _lastDrawnCenter)
            {
                ApplyTrail(chunk, _lastDrawnCenter);
                ApplySphere(chunk, newCenter);
                _lastDrawnCenter = newCenter;
            }
        }

        private void MoveAndBounce(float min, float max)
        {
            _currPosition += _direction * _stepMoveDistance;

            if (_currPosition.x < min)
            {
                _currPosition.x = min;
                _direction.x = -_direction.x;
            }
            else if (_currPosition.x > max)
            {
                _currPosition.x = max;
                _direction.x = -_direction.x;
            }

            if (_currPosition.y < min)
            {
                _currPosition.y = min;
                _direction.y = -_direction.y;
            }
            else if (_currPosition.y > max)
            {
                _currPosition.y = max;
                _direction.y = -_direction.y;
            }

            if (_currPosition.z < min)
            {
                _currPosition.z = min;
                _direction.z = -_direction.z;
            }
            else if (_currPosition.z > max)
            {
                _currPosition.z = max;
                _direction.z = -_direction.z;
            }

            _direction.Normalize();
        }

        private Vector3Int ToVoxelCenter(Vector3 p)
        {
            int x = Mathf.RoundToInt(p.x);
            int y = Mathf.RoundToInt(p.y);
            int z = Mathf.RoundToInt(p.z);

            return new Vector3Int(x, y, z);
        }

        private Color32 GetCurrColor()
        {
            float currPos = Mathf.PingPong(_currStep * _colorChangeSpeed, 1f);
            return _trailColor.Evaluate(currPos);
        }

        private void ApplyTrail(VoxelChunk chunk, Vector3Int center)
        {
            _chunkMaskApplier.Apply(chunk, new SphereMaskCreator(center, _radius),
                new MaskApplySettings
                {
                    IsModifyColors = true,
                    IsModifyMaterialId = true,
                    IsModifyVoxelType = true,
                    VoxType = VoxelType.Transparent,
                    Color = GetCurrColor(),
                    MaterialId = 1
                });
        }

        private void ApplySphere(VoxelChunk chunk, Vector3Int center)
        {
            _chunkMaskApplier.Apply(chunk, new SphereMaskCreator(center, _radius),
                new MaskApplySettings
                {
                    IsModifyColors = true,
                    IsModifyMaterialId = true,
                    IsModifyVoxelType = true,
                    VoxType = VoxelType.Solid,
                    Color = GetCurrColor(),
                    MaterialId = 0
                });
        }
    }
}


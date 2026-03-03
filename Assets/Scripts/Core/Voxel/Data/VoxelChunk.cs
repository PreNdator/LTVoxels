using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace LedenevTV.Voxel
{
    /// <summary>
    /// Stores voxel data for a single chunk in persistent <see cref="NativeArray{T}"/> buffers.
    /// Call <see cref="Dispose"/> when the chunk is no longer needed.
    /// </summary>
    public class VoxelChunk : IDisposable
    {
        public int VoxelsCount => VoxelTypes.Length;
        public int3 Size => _size;
        public Vector3Int SizeV3Int => new Vector3Int(_size.x, _size.y, _size.z);
        public NativeArray<VoxelType> VoxelTypes => _voxelTypes;
        public NativeArray<byte> MaterialIds => _materialIds;
        public NativeArray<Color32> Colors => _colors;
        public bool HasColors => _colors.IsCreated;
        public bool IsAllocated => _voxelTypes.IsCreated && _materialIds.IsCreated;

        private int _batchSize = 128;
        private NativeArray<VoxelType> _voxelTypes;
        private NativeArray<byte> _materialIds;
        private NativeArray<Color32> _colors;
        private int3 _size;

        /// <summary>
        /// Controls the preferred processing batch size for jobs/loops that operate on this chunk.
        /// Clamped to a minimum of 1.
        /// </summary>
        public int BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = Mathf.Max(1, value); }
        }

        /// <summary>
        /// Creates a chunk of the given size and allocates internal buffers.
        /// </summary>
        public VoxelChunk(int3 size, bool useColors = false)
        {
            Allocate(size, useColors);
        }

        /// <summary>
        /// Creates an empty chunk without allocation internal buffers.
        /// </summary>
        public VoxelChunk() { }

        /// <inheritdoc cref="VoxelChunk(int3,bool)"/>
        public VoxelChunk(Vector3Int size, bool useColors = false) : this(new int3(size.x, size.y, size.z), useColors) { }

        /// <inheritdoc cref="VoxelChunk(int3,bool)"/>
        public VoxelChunk(int size, bool useColors = false) : this(new int3(size, size, size), useColors) { }

        /// <summary>
        /// Deep copy of this chunk. Allocates new NativeArrays and copies all data.
        /// </summary>
        public VoxelChunk Clone()
        {
            VoxelChunk clone = new VoxelChunk(_size, HasColors);

            NativeArray<VoxelType>.Copy(_voxelTypes, clone._voxelTypes);
            NativeArray<byte>.Copy(_materialIds, clone._materialIds);

            if (HasColors)
            {
                NativeArray<Color32>.Copy(_colors, clone._colors);
            }

            clone.BatchSize = BatchSize;

            return clone;
        }


        ///<inheritdoc cref="Rebuild(int3,bool)"/>
        public void Rebuild(int size, bool useColors = false)
        {
            Rebuild(new int3(size, size, size), useColors);
        }

        /// <summary>
        /// Recreates internal buffers for the specified size. Existing buffers are disposed.
        /// </summary>
        public void Rebuild(int3 size, bool useColors = false)
        {
            DisposeNativeArrays();
            Allocate(size, useColors);
        }

        /// <summary>
        /// Sets the voxel type at the given local position.
        /// Returns false if the position is out of bounds.
        /// </summary>
        public bool TrySetVoxelType(Vector3Int position, VoxelType type)
        {
            int index = CoordToIndex(position);
            if (!IsValidIndex(index)) return false;

            _voxelTypes[index] = type;
            return true;
        }

        /// <summary>
        /// Sets the material Id at the given local position.
        /// Returns false if the position is out of bounds.
        /// </summary>
        public bool TrySetMaterialId(Vector3Int position, byte materialId)
        {
            int index = CoordToIndex(position);
            if (!IsValidIndex(index)) return false;

            _materialIds[index] = materialId;
            return true;
        }

        /// <summary>
        /// Sets the color at the given local position.
        /// Returns false if colors are disabled or the position is out of bounds.
        /// </summary>
        public bool TrySetColor(Vector3Int position, Color32 color)
        {
            if (!HasColors) return false;

            int index = CoordToIndex(position);
            if (!IsValidIndex(index)) return false;

            _colors[index] = color;
            return true;
        }

        /// <inheritdoc cref="TrySetVoxel(Vector3Int,VoxelType,byte,Color32)"/>
        public bool TrySetVoxel(Vector3Int position, VoxelType type, byte materialId)
        {
            int index = CoordToIndex(position);
            if (!IsValidIndex(index)) return false;

            _voxelTypes[index] = type;
            _materialIds[index] = materialId;
            return true;
        }

        /// <summary>
        /// Sets voxel type, material Id, and (optionally) color at the given local position.
        /// Returns false if the position is out of bounds. If colors are disabled, color is ignored.
        /// </summary>
        public bool TrySetVoxel(Vector3Int position, VoxelType type, byte materialId, Color32 color)
        {
            int index = CoordToIndex(position);
            if (!IsValidIndex(index)) return false;

            _voxelTypes[index] = type;
            _materialIds[index] = materialId;

            if (HasColors)
                _colors[index] = color;

            return true;
        }

        public bool IsValidIndex(int index)
        {
            return ChunkIndexing.IsValidIndex(index, _size);
        }

        public int3 IndexToCoord(int index)
        {
            return ChunkIndexing.IndexToCoord(index, _size);
        }

        public Vector3Int IndexToCoordVector3Int(int index)
        {
            return ChunkIndexing.IndexToCoord(index, SizeV3Int);
        }

        public int CoordToIndex(int x, int y, int z)
        {
            return ChunkIndexing.CoordToIndex(x, y, z, _size);
        }

        public int CoordToIndex(Vector3Int position)
        {
            return ChunkIndexing.CoordToIndex(position.x, position.y, position.z, _size);
        }

        public int CoordToIndex(int3 position)
        {
            return ChunkIndexing.CoordToIndex(position.x, position.y, position.z, _size);
        }

        public void Dispose()
        {
            DisposeNativeArrays();
        }

        private void DisposeNativeArrays()
        {
            if (_voxelTypes.IsCreated) _voxelTypes.Dispose();
            if (_materialIds.IsCreated) _materialIds.Dispose();
            if (_colors.IsCreated) _colors.Dispose();
        }

        private void Allocate(int3 size, bool useColors)
        {
            _size = math.max(size, 1);

            int voxelCount = _size.x * _size.y * _size.z;

            _voxelTypes = new NativeArray<VoxelType>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _materialIds = new NativeArray<byte>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            _colors = useColors
                ? new NativeArray<Color32>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory)
                : default;
        }
    }
}

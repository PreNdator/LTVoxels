using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask that selects voxels within a sphere defined by <see cref="Center"/> and <see cref="Radius"/>.
    /// </summary>
    public struct SphereMaskCreator : IChunkMaskCreator
    {
        public Vector3Int Center { get; }
        public float Radius { get; }

        public SphereMaskCreator(Vector3Int center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            int voxelCount = voxelChunk.VoxelTypes.Length;

            NativeArray<byte> byteMask = new NativeArray<byte>(voxelCount, Allocator.TempJob);
            NativeArray<int> indicesMask = default;

            try
            {
                SphereMaskJob job = new SphereMaskJob
                {
                    Mask = byteMask,
                    ChunkSize = voxelChunk.Size,
                    Center = new int3(Center.x, Center.y, Center.z),
                    RadiusSq = Radius * Radius
                };

                JobHandle jobHandle = job.Schedule(voxelCount, voxelChunk.BatchSize);
                jobHandle.Complete();

                indicesMask = MaskConverter.ByteMaskToIndices(byteMask, Allocator.TempJob);

                return indicesMask;
            }
            catch
            {
                if (indicesMask.IsCreated)
                    indicesMask.Dispose();

                throw;
            }
            finally
            {
                if (byteMask.IsCreated)
                    byteMask.Dispose();
            }
        }
    }
}


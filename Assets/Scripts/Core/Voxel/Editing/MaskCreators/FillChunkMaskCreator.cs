using Unity.Collections;
using Unity.Jobs;

namespace LedenevTV.Voxel.Editing
{
    /// <summary>
    /// Creates a mask that selects all voxels in the chunk.
    /// </summary>
    public struct FillChunkMaskCreator : IChunkMaskCreator
    {
        public NativeArray<int> CreateNewMask(VoxelChunk voxelChunk)
        {
            int voxelCount = voxelChunk.VoxelTypes.Length;

            NativeArray<int> voxelMask = new NativeArray<int>(voxelCount, Allocator.TempJob);

            try
            {
                FillAllChunkJob job = new FillAllChunkJob { Mask = voxelMask };

                JobHandle jobHandle = job.Schedule(voxelCount, voxelChunk.BatchSize);
                jobHandle.Complete();

                return voxelMask;
            }
            catch
            {
                if (voxelMask.IsCreated)
                    voxelMask.Dispose();

                throw;
            }
        }
    }
}


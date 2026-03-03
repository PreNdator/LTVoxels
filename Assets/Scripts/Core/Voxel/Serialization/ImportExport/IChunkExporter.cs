namespace LedenevTV.Voxel.Serialization
{
    public interface IChunkExporter
    {
        /// <summary>Serializes <paramref name="chunk"/> to the exporter format.</summary>
        byte[] ToBytes(VoxelChunk chunk);
    }

}
namespace LedenevTV.Voxel.Serialization
{
    public interface IBytesSource
    {
        /// <summary>
        /// Returns raw bytes from the specified source.
        /// </summary>
        byte[] GetBytes();
    }
}
using System;
using System.IO;


namespace LedenevTV.Voxel.Serialization
{
    public sealed class FileSystemBytesSource : IBytesSource
    {
        public string FilePath { get; }

        public FileSystemBytesSource(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is null or empty.", nameof(filePath));

            FilePath = filePath;
        }

        public byte[] GetBytes()
        {
            return File.ReadAllBytes(FilePath);
        }
    }
}
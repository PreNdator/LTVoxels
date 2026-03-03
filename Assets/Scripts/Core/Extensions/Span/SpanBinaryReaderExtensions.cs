using System;
using System.IO;

namespace LedenevTV.Extensions.Span
{
    public static class SpanBinaryReaderExtensions
    {
        /// <summary>
        /// Reads a byte and advances the position.
        /// </summary>
        /// <exception cref="InvalidDataException">
        /// Thrown when there is not enough data remaining in the span.
        /// </exception>
        public static byte ReadByte(this ReadOnlySpan<byte> data, ref int position)
        {
            if (position >= data.Length)
                throw new InvalidDataException("Unexpected end of data.");

            byte value = data[position];
            position += 1;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit signed integer and advances the position.
        /// </summary>
        /// <exception cref="InvalidDataException">
        /// Thrown when there is not enough data remaining in the span.
        /// </exception>
        public static int ReadInt32(this ReadOnlySpan<byte> data, ref int position)
        {
            if (position + sizeof(int) > data.Length)
                throw new InvalidDataException("Unexpected end of data.");

            int value = BitConverter.ToInt32(data.Slice(position, sizeof(int)));
            position += sizeof(int);
            return value;
        }

        /// <summary>
        /// Reads bytes into the destination span and advances the position.
        /// </summary>
        /// <exception cref="InvalidDataException">
        /// Thrown when there is not enough data remaining in the span.
        /// </exception>
        public static void ReadBytes(this ReadOnlySpan<byte> data, ref int position, Span<byte> destination)
        {
            if (position + destination.Length > data.Length)
                throw new InvalidDataException("Unexpected end of data.");

            data.Slice(position, destination.Length).CopyTo(destination);
            position += destination.Length;
        }

        /// <summary>
        /// Advances the position by the specified number of bytes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown when there is not enough data remaining in the span.
        /// </exception>
        public static void Skip(this ReadOnlySpan<byte> data, ref int position, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");

            if (position + count > data.Length)
                throw new InvalidDataException("Unexpected end of data.");

            position += count;
        }

        /// <summary>
        /// Reads a single line delimited by '\n' and advances the position.
        /// </summary>
        public static ReadOnlySpan<byte> ReadLine(this ReadOnlySpan<byte> data, ref int position)
        {
            if (position >= data.Length)
                return ReadOnlySpan<byte>.Empty;

            int start = position;

            while (position < data.Length)
            {
                byte b = data[position];
                if (b == (byte)'\n')
                {
                    int length = position - start;
                    position += 1;
                    return data.Slice(start, length);
                }

                position += 1;
            }

            int totalLength = data.Length - start;
            if (totalLength <= 0)
                return ReadOnlySpan<byte>.Empty;

            position = data.Length;
            return data.Slice(start, totalLength);
        }
    }
}
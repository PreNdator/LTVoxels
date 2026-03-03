using System;

namespace LedenevTV.Extensions.Span
{
    public static class SpanAsciiExtensions
    {
        private const int AsciiToLowerDelta = 'a' - 'A';

        /// <summary>
        /// Trims ASCII whitespace from both ends of the span.
        /// </summary>
        public static ReadOnlySpan<byte> TrimAscii(this ReadOnlySpan<byte> span)
        {
            int start = 0;
            int end = span.Length - 1;

            while (start <= end && IsWhiteSpace(span[start]))
                start++;

            while (end >= start && IsWhiteSpace(span[end]))
                end--;

            if (start > end)
                return ReadOnlySpan<byte>.Empty;

            return span.Slice(start, end - start + 1);
        }

        /// <summary>
        /// Returns true if the span contains only ASCII whitespace.
        /// </summary>
        public static bool IsEmptyOrWhiteSpaceAscii(this ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (!IsWhiteSpace(span[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares ASCII bytes with a given string literally (case-sensitive).
        /// </summary>
        public static bool EqualsAscii(this ReadOnlySpan<byte> span, string text)
        {
            if (span.Length != text.Length)
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                if (span[i] != (byte)text[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if span starts with given ASCII string (case-sensitive).
        /// </summary>
        public static bool StartsWithAscii(this ReadOnlySpan<byte> span, string text)
        {
            if (span.Length < text.Length)
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                if (span[i] != (byte)text[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Case-insensitive ASCII compare (A–Z only).
        /// </summary>
        public static bool EqualsAsciiIgnoreCase(this ReadOnlySpan<byte> span, string text)
        {

            if (span.Length != text.Length)
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                byte b = span[i];
                char c = text[i];

                if (b >= 'A' && b <= 'Z')
                    b = (byte)(b + AsciiToLowerDelta);

                if (c >= 'A' && c <= 'Z')
                    c = (char)(c + AsciiToLowerDelta);

                if (b != (byte)c)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the next token separated by ASCII whitespace and advances the index.
        /// </summary>
        public static bool TryGetNextTokenAscii(this ReadOnlySpan<byte> line, ref int index, out ReadOnlySpan<byte> token)
        {
            int length = line.Length;

            while (index < length && IsWhiteSpace(line[index]))
                index++;

            if (index >= length)
            {
                token = default;
                return false;
            }

            int start = index;

            while (index < length && !IsWhiteSpace(line[index]))
                index++;

            int tokenLength = index - start;
            token = line.Slice(start, tokenLength);
            return true;
        }

        /// <summary>
        /// Tries to parse ASCII integer from span.
        /// </summary>
        public static bool TryParseIntAscii(this ReadOnlySpan<byte> span, out int value)
        {
            value = 0;
            span = span.TrimAscii();
            if (span.Length == 0)
                return false;

            int sign = 1;
            int i = 0;

            byte c = span[0];
            if (c == (byte)'-')
            {
                sign = -1;
                i++;
            }

            if (i >= span.Length)
                return false;

            int result = 0;
            for (; i < span.Length; i++)
            {
                c = span[i];
                if (c < (byte)'0' || c > (byte)'9')
                    return false;

                result = result * 10 + (c - (byte)'0');
            }

            value = result * sign;
            return true;
        }

        /// <summary>
        /// Tries to parse ASCII byte (0..255) from span.
        /// </summary>
        public static bool TryParseByteAscii(this ReadOnlySpan<byte> span, out byte value)
        {
            if (!span.TryParseIntAscii(out int tmp))
            {
                value = 0;
                return false;
            }

            if (tmp < byte.MinValue || tmp > byte.MaxValue)
            {
                value = 0;
                return false;
            }

            value = (byte)tmp;
            return true;
        }

        private static bool IsWhiteSpace(byte b)
        {
            return b == (byte)' ' ||
                   b == (byte)'\t' ||
                   b == (byte)'\r' ||
                   b == (byte)'\n';
        }

    }
}
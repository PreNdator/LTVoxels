using LedenevTV.Extensions.Span;
using NUnit.Framework;
using System;
using System.Text;

namespace LedenevTV.Tests.Extension
{
    public class SpanAsciiExtensionsTests
    {
        private static ReadOnlySpan<byte> Ascii(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        [Test]
        public void TrimAscii_RemovesLeadingAndTrailingWhitespace()
        {
            ReadOnlySpan<byte> span = Ascii("  \t\r\nabc  \n");
            ReadOnlySpan<byte> trimmed = span.TrimAscii();

            Assert.AreEqual("abc", Encoding.ASCII.GetString(trimmed));
        }

        [Test]
        public void TrimAscii_AllWhitespace_ReturnsEmpty()
        {
            ReadOnlySpan<byte> span = Ascii(" \t\r\n  ");
            ReadOnlySpan<byte> trimmed = span.TrimAscii();

            Assert.AreEqual(0, trimmed.Length);
        }

        [Test]
        public void TrimAscii_NoWhitespace_ReturnsSameContent()
        {
            ReadOnlySpan<byte> span = Ascii("abc");
            ReadOnlySpan<byte> trimmed = span.TrimAscii();

            Assert.AreEqual("abc", Encoding.ASCII.GetString(trimmed));
        }

        [Test]
        public void IsEmptyOrWhiteSpaceAscii_TrueForOnlyWhitespace()
        {
            ReadOnlySpan<byte> span = Ascii(" \t\r\n ");
            Assert.IsTrue(span.IsEmptyOrWhiteSpaceAscii());
        }

        [Test]
        public void IsEmptyOrWhiteSpaceAscii_TrueForEmpty()
        {
            ReadOnlySpan<byte> span = Ascii(string.Empty);
            Assert.IsTrue(span.IsEmptyOrWhiteSpaceAscii());
        }

        [Test]
        public void IsEmptyOrWhiteSpaceAscii_FalseForNonWhitespace()
        {
            ReadOnlySpan<byte> span = Ascii(" a ");
            Assert.IsFalse(span.IsEmptyOrWhiteSpaceAscii());
        }

        [Test]
        public void EqualsAscii_TrueForSameContent()
        {
            ReadOnlySpan<byte> span = Ascii("ply");
            Assert.IsTrue(span.EqualsAscii("ply"));
        }

        [Test]
        public void EqualsAscii_FalseForDifferentCase()
        {
            ReadOnlySpan<byte> span = Ascii("ply");
            Assert.IsFalse(span.EqualsAscii("PLY"));
        }

        [Test]
        public void EqualsAscii_FalseForDifferentLength()
        {
            ReadOnlySpan<byte> span = Ascii("plyX");
            Assert.IsFalse(span.EqualsAscii("ply"));
        }

        [Test]
        public void StartsWithAscii_TrueForPrefix()
        {
            ReadOnlySpan<byte> span = Ascii("ply something");
            Assert.IsTrue(span.StartsWithAscii("ply"));
        }

        [Test]
        public void StartsWithAscii_FalseIfSpanShorterThanPrefix()
        {
            ReadOnlySpan<byte> span = Ascii("pl");
            Assert.IsFalse(span.StartsWithAscii("ply"));
        }

        [Test]
        public void EqualsAsciiIgnoreCase_TrueForSameIgnoringCase()
        {
            ReadOnlySpan<byte> span = Ascii("PlY");
            Assert.IsTrue(span.EqualsAsciiIgnoreCase("ply"));
        }

        [Test]
        public void EqualsAsciiIgnoreCase_FalseForDifferentContent()
        {
            ReadOnlySpan<byte> span = Ascii("plx");
            Assert.IsFalse(span.EqualsAsciiIgnoreCase("ply"));
        }

        [Test]
        public void TryGetNextTokenAscii_ParsesTokensSequentially()
        {
            ReadOnlySpan<byte> line = Ascii("  foo   bar\tbaz  ");
            int index = 0;

            Assert.IsTrue(line.TryGetNextTokenAscii(ref index, out ReadOnlySpan<byte> token1));
            Assert.AreEqual("foo", Encoding.ASCII.GetString(token1));

            Assert.IsTrue(line.TryGetNextTokenAscii(ref index, out ReadOnlySpan<byte> token2));
            Assert.AreEqual("bar", Encoding.ASCII.GetString(token2));

            Assert.IsTrue(line.TryGetNextTokenAscii(ref index, out ReadOnlySpan<byte> token3));
            Assert.AreEqual("baz", Encoding.ASCII.GetString(token3));

            Assert.IsFalse(line.TryGetNextTokenAscii(ref index, out _));
        }

        [Test]
        public void TryGetNextTokenAscii_WhitespaceOnly_ReturnsFalse()
        {
            ReadOnlySpan<byte> line = Ascii("   \t\r\n  ");
            int index = 0;

            Assert.IsFalse(line.TryGetNextTokenAscii(ref index, out ReadOnlySpan<byte> token));
            Assert.AreEqual(0, token.Length);
        }

        [Test]
        public void TryParseIntAscii_ParsesPositive()
        {
            ReadOnlySpan<byte> span = Ascii("123");
            Assert.IsTrue(span.TryParseIntAscii(out int value));
            Assert.AreEqual(123, value);
        }

        [Test]
        public void TryParseIntAscii_ParsesNegative()
        {
            ReadOnlySpan<byte> span = Ascii("-45");
            Assert.IsTrue(span.TryParseIntAscii(out int value));
            Assert.AreEqual(-45, value);
        }

        [Test]
        public void TryParseIntAscii_TrimsWhitespace()
        {
            ReadOnlySpan<byte> span = Ascii("  7\t");
            Assert.IsTrue(span.TryParseIntAscii(out int value));
            Assert.AreEqual(7, value);
        }

        [Test]
        public void TryParseIntAscii_InvalidInput_ReturnsFalse()
        {
            ReadOnlySpan<byte> span = Ascii("12x");
            Assert.IsFalse(span.TryParseIntAscii(out int value));
            Assert.AreEqual(0, value);
        }

        [Test]
        public void TryParseByteAscii_ValidRange_ReturnsTrue()
        {
            ReadOnlySpan<byte> span = Ascii("255");
            Assert.IsTrue(span.TryParseByteAscii(out byte value));
            Assert.AreEqual((byte)255, value);
        }

        [Test]
        public void TryParseByteAscii_OutOfRange_ReturnsFalse()
        {
            ReadOnlySpan<byte> span = Ascii("300");
            Assert.IsFalse(span.TryParseByteAscii(out byte value));
            Assert.AreEqual((byte)0, value);
        }

        [Test]
        public void TryParseByteAscii_Negative_ReturnsFalse()
        {
            ReadOnlySpan<byte> span = Ascii("-1");
            Assert.IsFalse(span.TryParseByteAscii(out byte value));
            Assert.AreEqual((byte)0, value);
        }
    }
}
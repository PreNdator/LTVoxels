using LedenevTV.Extensions.Span;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace LedenevTV.Tests.Extension
{
    public class SpanBinaryReaderExtensionsTests
    {

        private static ReadOnlySpan<byte> Ascii(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        [Test]
        public void ReadByte_ReadsAndAdvancesPosition()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(10, 20, 30);
            int position = 0;

            byte b1 = data.ReadByte(ref position);
            byte b2 = data.ReadByte(ref position);

            Assert.AreEqual(10, b1);
            Assert.AreEqual(20, b2);
            Assert.AreEqual(2, position);
        }

        [Test]
        public void ReadByte_ThrowsOnEndOfData()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(10);
            int position = 1;

            try
            {
                data.ReadByte(ref position);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void ReadInt32_ReadsLittleEndianInt32()
        {
            int original = 123456789;
            byte[] bytes = BitConverter.GetBytes(original);

            ReadOnlySpan<byte> data = new ReadOnlySpan<byte>(bytes);
            int position = 0;

            int value = data.ReadInt32(ref position);

            Assert.AreEqual(original, value);
            Assert.AreEqual(sizeof(int), position);
        }

        [Test]
        public void ReadInt32_ThrowsIfNotEnoughBytes()
        {
            byte[] bytes = { 1, 2 };
            ReadOnlySpan<byte> data = new ReadOnlySpan<byte>(bytes);
            int position = 0;

            try
            {
                data.ReadInt32(ref position);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void ReadBytes_ReadsIntoDestinationAndAdvancesPosition()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(1, 2, 3, 4, 5);
            int position = 1;

            byte[] dest = new byte[3];
            data.ReadBytes(ref position, dest);

            CollectionAssert.AreEqual(new byte[] { 2, 3, 4 }, dest);
            Assert.AreEqual(4, position);
        }

        [Test]
        public void ReadBytes_ThrowsIfNotEnoughData()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(1, 2, 3);
            int position = 2;

            byte[] dest = new byte[2];

            try
            {
                data.ReadBytes(ref position, dest);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void Skip_AdvancesPosition()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(1, 2, 3, 4, 5);
            int position = 1;

            data.Skip(ref position, 3);

            Assert.AreEqual(4, position);
        }

        [Test]
        public void Skip_ThrowsOnNegativeCount()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(1, 2, 3);
            int position = 0;

            try
            {
                data.Skip(ref position, -1);
                Assert.Fail("Expected ArgumentOutOfRangeException.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test]
        public void Skip_ThrowsIfNotEnoughData()
        {
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(1, 2, 3);
            int position = 2;

            try
            {
                data.Skip(ref position, 2);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void ReadLine_ReadsUntilNewLineAndAdvancesPosition()
        {
            ReadOnlySpan<byte> data = Ascii("abc\ndef\n");
            int position = 0;

            ReadOnlySpan<byte> line1 = data.ReadLine(ref position);
            ReadOnlySpan<byte> line2 = data.ReadLine(ref position);
            ReadOnlySpan<byte> line3 = data.ReadLine(ref position);

            Assert.AreEqual("abc", Encoding.ASCII.GetString(line1));
            Assert.AreEqual("def", Encoding.ASCII.GetString(line2));
            Assert.AreEqual(0, line3.Length);
        }

        [Test]
        public void ReadLine_LastLineWithoutNewLine_ReturnsRemainingBytes()
        {
            ReadOnlySpan<byte> data = Ascii("lastline");
            int position = 0;

            ReadOnlySpan<byte> line = data.ReadLine(ref position);

            Assert.AreEqual("lastline", Encoding.ASCII.GetString(line));
            Assert.AreEqual(data.Length, position);

            ReadOnlySpan<byte> empty = data.ReadLine(ref position);
            Assert.AreEqual(0, empty.Length);
        }

        [Test]
        public void ReadLine_KeepsCarriageReturnBeforeNewLine()
        {
            ReadOnlySpan<byte> data = Ascii("abc\r\ndef\n");
            int position = 0;

            ReadOnlySpan<byte> line1 = data.ReadLine(ref position);
            ReadOnlySpan<byte> line2 = data.ReadLine(ref position);

            string rawLine1 = Encoding.ASCII.GetString(line1);
            string rawLine2 = Encoding.ASCII.GetString(line2);

            Assert.AreEqual("abc\r", rawLine1);
            Assert.AreEqual("def", rawLine2);
        }
    }
}
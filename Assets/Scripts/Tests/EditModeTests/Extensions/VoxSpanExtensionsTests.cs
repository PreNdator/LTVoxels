using LedenevTV.Extensions.Span;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace LedenevTV.Tests.Extension
{
    public class VoxSpanExtensionsTests
    {

        [Test]
        public void ReadVoxSize_ReadsDimensionsAndAdvancesPosition()
        {
            int3 expected = new int3(10, 20, 30);

            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(expected.x));
            buffer.AddRange(BitConverter.GetBytes(expected.y));
            buffer.AddRange(BitConverter.GetBytes(expected.z));

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            int3 size = data.ReadVoxSize(ref position);

            Assert.AreEqual(expected.x, size.x);
            Assert.AreEqual(expected.y, size.y);
            Assert.AreEqual(expected.z, size.z);
            Assert.AreEqual(3 * sizeof(int), position);
        }

        [Test]
        public void ReadVoxSize_ThrowsOnInvalidDimensions()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(0));
            buffer.AddRange(BitConverter.GetBytes(1));
            buffer.AddRange(BitConverter.GetBytes(1));

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            try
            {
                data.ReadVoxSize(ref position);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void ReadVoxXyzi_ReadsVoxelsAndAdvancesPosition()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(2));
            buffer.Add(1); buffer.Add(2); buffer.Add(3); buffer.Add(4);
            buffer.Add(10); buffer.Add(20); buffer.Add(30); buffer.Add(40);

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            List<(byte x, byte y, byte z, byte colorIndex)> voxels =
                new List<(byte x, byte y, byte z, byte colorIndex)>();

            data.ReadVoxXyzi(ref position, voxels);

            Assert.AreEqual(2, voxels.Count);
            Assert.AreEqual(sizeof(int) + 2 * 4, position);

            Assert.AreEqual(1, voxels[0].x);
            Assert.AreEqual(2, voxels[0].y);
            Assert.AreEqual(3, voxels[0].z);
            Assert.AreEqual(4, voxels[0].colorIndex);

            Assert.AreEqual(10, voxels[1].x);
            Assert.AreEqual(20, voxels[1].y);
            Assert.AreEqual(30, voxels[1].z);
            Assert.AreEqual(40, voxels[1].colorIndex);
        }

        [Test]
        public void ReadVoxXyzi_ZeroVoxels_DoesNothing()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(0));

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            List<(byte x, byte y, byte z, byte colorIndex)> voxels =
                new List<(byte x, byte y, byte z, byte colorIndex)>();

            data.ReadVoxXyzi(ref position, voxels);

            Assert.AreEqual(0, voxels.Count);
            Assert.AreEqual(sizeof(int), position);
        }

        [Test]
        public void ReadVoxXyzi_ThrowsIfNotEnoughData()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(1));

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            List<(byte x, byte y, byte z, byte colorIndex)> voxels =
                new List<(byte x, byte y, byte z, byte colorIndex)>();

            try
            {
                data.ReadVoxXyzi(ref position, voxels);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }

        [Test]
        public void ReadVoxRgbaPalette_Reads256ColorsAndAdvancesPosition()
        {
            const int PaletteSize = 256;

            List<byte> buffer = new List<byte>(PaletteSize * 4);

            for (int i = 0; i < PaletteSize; i++)
            {
                byte r = (byte)i;
                byte g = (byte)(255 - i);
                byte b = (byte)(i ^ 0x55);
                byte a = (byte)(i == 0 ? 0 : 255);

                buffer.Add(r);
                buffer.Add(g);
                buffer.Add(b);
                buffer.Add(a);
            }

            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer.ToArray());
            int position = 0;

            Color32[] palette = data.ReadVoxRgbaPalette(ref position);

            Assert.AreEqual(PaletteSize, palette.Length);
            Assert.AreEqual(PaletteSize * 4, position);

            Assert.AreEqual(0, palette[0].r);
            Assert.AreEqual(255, palette[0].g);
            Assert.AreEqual((byte)(0 ^ 0x55), palette[0].b);
            Assert.AreEqual(0, palette[0].a);

            int last = PaletteSize - 1;

            Assert.AreEqual((byte)last, palette[last].r);
            Assert.AreEqual((byte)(255 - last), palette[last].g);
            Assert.AreEqual((byte)(last ^ 0x55), palette[last].b);
            Assert.AreEqual(255, palette[last].a);
        }

        [Test]
        public void ReadVoxRgbaPalette_ThrowsIfNotEnoughData()
        {
            byte[] buffer = new byte[10];
            ReadOnlySpan<byte> data = ExtensionTestsHelper.Bytes(buffer);
            int position = 0;

            try
            {
                data.ReadVoxRgbaPalette(ref position);
                Assert.Fail("Expected InvalidDataException.");
            }
            catch (InvalidDataException)
            {
            }
        }
    }
}
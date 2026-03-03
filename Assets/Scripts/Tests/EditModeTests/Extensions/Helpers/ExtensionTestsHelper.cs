using System;

namespace LedenevTV.Tests.Extension
{
    public class ExtensionTestsHelper
    {
        public static ReadOnlySpan<byte> Bytes(params byte[] data)
        {
            return new ReadOnlySpan<byte>(data);
        }
    }
}
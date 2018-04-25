using System;
using System.Collections;
using System.Drawing;

namespace DontWanaCrypt
{
    public static class BinaryUtils
    {
        public static byte[] ImageToBytes(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static BitArray Prepend(this BitArray current, BitArray before)
        {
            var bools = new bool[current.Count + before.Count];
            before.CopyTo(bools, 0);
            current.CopyTo(bools, before.Count);
            return new BitArray(bools);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;
using System.Threading;

namespace DontWanaCrypt
{
    public static class BinaryUtils
    {
        public static BitArray Prepend(this BitArray current, BitArray before)
        {
            var bools = new bool[current.Count + before.Count];
            before.CopyTo(bools, 0);
            current.CopyTo(bools, before.Count);
            return new BitArray(bools);
        }
    }
    public static class BinaryConverter
    {
        public static BitArray ToBinary(this int numeral) => new BitArray(new[] { numeral });

        public static int ToNumeral(this BitArray binary)
        {
            if (binary == null)
                throw new ArgumentNullException("binary");
            if (binary.Length > 32)
                throw new ArgumentException("must be at most 32 bits long");

            var result = new int[1];
            binary.CopyTo(result, 0);
            return result[0];
        }
    }

    class Program
    {
        

        static Bitmap EncryptBitmap(string path, BitArray messageBits)
        {
            // should be 32 bits
            BitArray messageSizeBits = BinaryConverter.ToBinary(messageBits.Length);

            messageBits = messageBits.Prepend(messageSizeBits);

            int bitsPerPixel = 3;
            int i = 0;

            var bitmap = new Bitmap(path);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color p = bitmap.GetPixel(x, y);
                    var rgb = new Dictionary<char, byte> {
                        { 'r', p.R },
                        { 'g', p.G },
                        { 'b', p.B },
                    };
                    byte a = p.A;

                    foreach (char key in new char[] { 'r', 'g', 'b' })
                    {
                        var v = rgb[key];
                        if (messageBits[i] && v % 2 == 0)
                            rgb[key] = (byte)(v + 1);
                        else if (!messageBits[i] && v % 2 == 1)
                            rgb[key] = (byte)(v - 1);
                        i++;

                        Color newColor = Color.FromArgb(a, rgb['r'], rgb['g'], rgb['b']);
                        bitmap.SetPixel(x, y, newColor);

                        if (i >= messageBits.Length)
                            return bitmap;
                    }
                }
            }
            return bitmap;
        }

        static BitArray DecryptBitmap(Bitmap bitmap)
        {
            int i = 0;
            BitArray messageLengthBits = new BitArray(32);
            int messageLength = 0;

            var bits = new List<bool>();
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    
                    Color p = bitmap.GetPixel(x, y);
                    byte a = p.A;
                    var rgb = new Dictionary<char, byte> {
                        { 'r', p.R },
                        { 'g', p.G },
                        { 'b', p.B },
                    };

                    foreach (char key in new char[] { 'r', 'g', 'b' })
                    {
                        var v = rgb[key];
                        if (i >= 32)
                            bits.Add(v % 2 == 1);
                        else
                            messageLengthBits[i] = v % 2 == 1;
                        i++;

                        if (i >= 31)
                            messageLength = BinaryConverter.ToNumeral(messageLengthBits);

                        if (i >= 32 && i - 32 >= messageLength)
                            return new BitArray(bits.ToArray());
                    }

                }
            }
            return new BitArray(bits.ToArray());
        }

        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        // TODO: encode message length too
        static void Main(string[] args)
        {
            var path = "C:/Users/domin/wallpaper.jpg";
            var pathOut = "C:/Users/domin/wallpaper2.jpg";

            Console.Write("Data: ");
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            BitArray messageBits = new BitArray(messageBytes);

            // encrypt
            Console.WriteLine("Encrypting...");
            var encryptedBitmap = EncryptBitmap(path, messageBits);
            encryptedBitmap.Save(pathOut);


            Console.WriteLine("Decrypting...");
            var decrypted = DecryptBitmap(encryptedBitmap);

            byte[] decryptedMessageBytes = BitArrayToByteArray(decrypted);
            string decryptedAscii = Encoding.ASCII.GetString(decryptedMessageBytes);
            Console.WriteLine(decryptedAscii);
            Console.WriteLine("Done...");

            Console.WriteLine();

            Console.ReadLine();
        }
    }
}

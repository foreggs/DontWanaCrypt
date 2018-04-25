using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace DontWanaCrypt
{
    enum DataType: byte
    {
        PlainText,
        Image,
    }

   class Program
    {
        static void PrintBanner()
        {
            Console.WriteLine(@" _____              _ _    __          __                  _____                  _");
            Console.WriteLine(@"|  __ \            ( ) |   \ \        / /                 / ____|                | |");
            Console.WriteLine(@"| |  | | ___  _ __ |/| |_   \ \  /\  / /_ _ _ __   __ _  | |     _ __ _   _ _ __ | |_");
            Console.WriteLine(@"| |  | |/ _ \| '_ \  | __|   \ \/  \/ / _` | '_ \ / _` | | |    | '__| | | | '_ \| __|");
            Console.WriteLine(@"| |__| | (_) | | | | | |_     \  /\  / (_| | | | | (_| | | |____| |  | |_| | |_) | |_");
            Console.WriteLine(@"|_____/ \___/|_| |_|  \__|     \/  \/ \__,_|_| |_|\__,_|  \_____|_|   \__, | .__/ \__|");
            Console.WriteLine(@"                                                                      __ / | |");
            Console.WriteLine(@"                                                                     |____/|_|");
        }

        public static string GetPassword()
        {
            string pwd = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd = pwd.Remove(pwd.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }

        static Bitmap HideMessageInBitmap(Bitmap bitmap, BitArray messageBits, DataType dataType)
        {
            // should be 32 bits
            BitArray messageSizeBits = BinaryConverter.ToBinary(messageBits.Length);
            // should be 8 bits
            BitArray dataTypeSizeBits = BinaryConverter.ToBinary((byte)dataType);

            messageBits = messageBits.Prepend(dataTypeSizeBits).Prepend(messageSizeBits);

            int i = 0;

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

        static (BitArray data, DataType type) GetDataFromBitmap(Bitmap bitmap)
        {
            int i = 0;
            BitArray messageLengthBits = new BitArray(32);
            BitArray messageTypeBits = new BitArray(8);
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
                        if (i >= 40)
                            bits.Add(v % 2 == 1);
                        else if (i >= 32)
                        {
                            messageTypeBits[i - 32] = v % 2 == 1;
                        }
                        else
                            messageLengthBits[i] = v % 2 == 1;
                        i++;

                        if (i >= 31)
                            messageLength = BinaryConverter.ToNumeral(messageLengthBits);

                        if (i >= 40 && i - 40 >= messageLength)
                        {
                            return (
                                data: new BitArray(bits.ToArray()),
                                type: (DataType)BinaryConverter.ToNumeral(messageTypeBits)
                            );
                        }
                    }

                }
            }
            return (
                data: new BitArray(bits.ToArray()),
                type: (DataType)BinaryConverter.ToNumeral(messageTypeBits)
            );
        }

        // TODO: encode message length too
        static void Main(string[] args)
        {
            Validator PathValidator = new Validator("Invalid path", v => File.Exists(v));
            Validator ImageValidator = new Validator("Invalid image extension", v => (new string[] { "png", "jpg" }).Contains(v.Split('.')[v.Split('.').Length - 1]));
            Validator ImageLosslessValidator = new Validator("Must use lossless compression", v => (new string[] { "png", "jpg" }).Contains(v.Split('.')[v.Split('.').Length - 1]));

            PrintBanner();
            
            string choice = OneInput.Input(new InputConfiguration {
                Pre = "Would you like to: \n" +
                "\t1. Hide data in an image\n" +
                "\t2. Get hidden data from an image",
                Label = "Choice",
                Validators = new Validator[] { new Validator("Valid option", s => s == "1" || s == "2") },
                ShowValidationErrors = false
            });

            if (choice == "1")
            {
                string path = OneInput.Input(new InputConfiguration
                {
                    Label = "Original image path",
                    Validators = new Validator[] { PathValidator, ImageValidator }
                });
                string pathOut = OneInput.Input(new InputConfiguration
                {
                    Label = "Output image path",
                    Validators = new Validator[] { ImageValidator, ImageLosslessValidator}
                });

                string password = OneInput.Input(new InputConfiguration
                {
                    Label = "Password",
                    Type = InputType.Password,
                    Validators = new Validator[] { new Validator("Must be at least 8 characters", v => v.Length >= 8) }
                });

                Console.WriteLine();

                Console.Write("Data type (image or text): ");
                string inputType = Console.ReadLine().ToLower();

                byte[] messageBytes = new byte[] { };

                if (inputType == "image")
                {
                    Console.Write("Hidden image path: ");
                    string hiddenImagePath = Console.ReadLine();
                    Bitmap hiddenBitmap = new Bitmap(hiddenImagePath);
                    messageBytes = BinaryUtils.ImageToBytes(hiddenBitmap);

                } else if (inputType == "text")
                {
                    Console.Write("Message: ");
                    string message = Console.ReadLine();
                    messageBytes = Encoding.ASCII.GetBytes(message);
                }
                else
                {
                    Console.WriteLine("Invalid data type.");
                    return;
                }
                
                byte[] encryptedMessageBytes = Aes.Encrypt(messageBytes, Encoding.ASCII.GetBytes(password));
                BitArray encryptedMessageBits = new BitArray(encryptedMessageBytes);

                //TODO progress bar
                var bitmap = new Bitmap(path);
                var encryptedBitmap = HideMessageInBitmap(bitmap, encryptedMessageBits, DataType.PlainText);
                encryptedBitmap.Save(pathOut);
                encryptedBitmap.Dispose();
            }
            else if (choice == "2")
            {
                Console.Clear();
                /*
                string path = OneInput.Input(new InputConfiguration
                {
                    Label = "Original image path",
                    Validators = new Validator[] { PathValidator, ImageValidator, ImageLosslessValidator }
                });


                Console.Write("Password: ");
                var password = OneInput.Input(new InputConfiguration {
                    Label = "Password",
                    Type = InputType.Password
                });
                */
                string path = "C:/users/domin/wallpaper2.png";
                var password = "fsociety";
                var encryptedBitmap = new Bitmap(path);

                Console.WriteLine("Decrypting...");
                var encrypted = GetDataFromBitmap(encryptedBitmap);
                byte[] messageBytes = BinaryUtils.BitArrayToByteArray(encrypted.data);
                try
                {
                    byte[] decryptedMessage = Aes.Decrypt(messageBytes, Encoding.ASCII.GetBytes(password));
                    switch(encrypted.type)
                    {
                        case DataType.PlainText:
                            string decryptedAscii = Encoding.ASCII.GetString(decryptedMessage);
                            Console.WriteLine(decryptedAscii);
                            break;
                        case DataType.Image:
                            Console.Write("Image Detected. Path to image output: ");
                            string pathOut = Console.ReadLine();
                            Bitmap bmp;
                            using (var ms = new MemoryStream(decryptedMessage))
                            {
                                bmp = new Bitmap(ms);
                                bmp.Save(pathOut);
                            }
                            Process.Start(pathOut);
                            break;
                    }

                } catch (CryptographicException) {
                    Console.WriteLine("Invalid password");
                }
            }

            Console.ReadLine();
        }
    }
}

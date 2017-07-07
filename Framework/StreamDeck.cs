using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using HidSharp;

namespace StreamDeck.Framework
{
    public delegate void KeyPressedEventHandler(KeyEventArgs args);
    public delegate void KeyReleasedEventHandler(KeyEventArgs args);

    public class KeyEventArgs : EventArgs
    {
        public readonly int KeyId;
        public KeyEventArgs(int keyId)
        {
            KeyId = keyId;
        }
    }

    public static class StreamDeck
    {
        private const int vendorId = 0x0fd9;
        private const int productId = 0x0060;
        private const int numKeys = 15;
        private const int pagePacketSize = 8191;
        private const int numFirstPagePixels = 2583;
        private const int numSecondPagePixels = 2601;
        private const int iconSize = 72;
        private const int numTotalPixels = numFirstPagePixels + numSecondPagePixels;

        private static readonly HidDevice streamDeckDevice;
        private static readonly HidStream deviceStream;
        private static readonly Thread readThread;

        private static readonly bool[] keyState;

        public static event KeyPressedEventHandler KeyPressed;
        private static void OnKeyPressed(KeyEventArgs args)
        {
            KeyPressed?.Invoke(args);
        }

        public static event KeyReleasedEventHandler KeyReleased;
        private static void OnKeyReleased(KeyEventArgs args)
        {
            KeyReleased?.Invoke(args);
        }

        static StreamDeck()
        {
            streamDeckDevice = FindUSBDevice();
            deviceStream = streamDeckDevice.Open();
            if (deviceStream == null)
            {
                Logger.Log("StreamDeck", "Could not open data stream to StreamDeck device!", LoggerVerbosity.Error);
                Environment.Exit(1);
            }

            Logger.Log("StreamDeck", "Opened data stream on StreamDeck USB device.");

            AppDomain.CurrentDomain.ProcessExit += Shutdown;

            keyState = new bool[numKeys];

            readThread = new Thread(Read);
            readThread.Start();
        }

        private static void Read()
        {
            using (deviceStream)
            {
                while (true)
                {
                    byte[] deviceBytes = new byte[streamDeckDevice.MaxInputReportLength];
                    int count;

                    try
                    {
                        count = deviceStream.Read(deviceBytes, 0, deviceBytes.Length);
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }

                    if (count <= 0) continue;

                    byte[] data = deviceBytes.Skip(1).Take(deviceBytes.Length - 2).ToArray();
                    for (int i = 0; i < numKeys; i++)
                    {
                        bool keyPressed = Convert.ToBoolean(data[i]);
                        if (keyPressed != keyState[i])
                        {
                            if (keyPressed)
                            {
                                OnKeyPressed(new KeyEventArgs(i));
                            }
                            else
                            {
                                OnKeyReleased(new KeyEventArgs(i));
                            }
                        }

                        keyState[i] = keyPressed;
                    }
                }
            }
        }

        private static HidDevice FindUSBDevice()
        {
            HidDeviceLoader loader = new HidDeviceLoader();
            HidDevice device = loader.GetDeviceOrDefault(vendorId, productId);
            if (device != null)
            {
                Logger.Log("StreamDeck", "Found StreamDeck USB device!");
                return device;
            }

            Logger.Log("StreamDeck", "Could not find StreamDeck USB device!", LoggerVerbosity.Error);
            return null;
        }


        private static void Shutdown(object sender, EventArgs eventArgs)
        {
            Logger.Log("StreamDeck", "Shutting down StreamDack handler!");
            readThread.Abort();
        }

        public static void FillColour(int key, Colour colour)
        {
            if (!IsValidKey(key))
            {
                Logger.Log("StreamDeck", $"FillColour: Received invalid key(id={key}) to fill colour. ");
                return;
            }

            byte[] pixels = {colour.B, colour.G, colour.R};
            WritePage1(key, Alloc(numFirstPagePixels * 3, pixels));
            WritePage2(key, Alloc(numSecondPagePixels * 3, pixels));
        }

        private static byte[] Alloc(int size, IReadOnlyList<byte> fill)
        {
            byte[] buffer = new byte[size];
            for (int i = 0; i < buffer.Length; i+=fill.Count)
            {
                for (int j = 0; j < fill.Count; j++)
                {
                    buffer[i + j] = fill[j];
                }
            }

            return buffer; 
        }

        private static bool IsValidKey(int key)
        {
            return key > 0 || key < numKeys - 1;
        }

        private static void Write(byte[] buffer)
        {
            deviceStream.Write(buffer);
        }

        private static void WritePage1(int key, byte[] buffer)
        {
            byte[] header =
            {
                0x02, 0x01, 0x01, 0x00, 0x00, (byte)(key + 1), 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x42, 0x4d, 0xf6, 0x3c, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x48, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xc0, 0x3c, 0x00, 0x00, 0xc4, 0x0e,
                0x00, 0x00, 0xc4, 0x0e, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            byte[] packet = PadToLength(Concat(header, buffer), pagePacketSize);
            Write(packet);
        }

        private static void WritePage2(int key, byte[] buffer)
        {
            byte[] header = {0x02, 0x01, 0x02, 0x00, 0x01, (byte) (key + 1)};
            byte[] packet = PadToLength(Concat(header, Pad(10), buffer), pagePacketSize);
            Write(packet);
        }

        private static byte[] PadToLength(byte[] buffer, int length)
        {
            return Concat(buffer, Pad(length - buffer.Length));
        }

        private static byte[] Pad(int length)
        {
            return new byte[length];
        }

        private static byte[] Concat(params byte[][] arrays)
        {
            byte[] data = new byte[arrays.Sum(arr => arr.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, data, offset, array.Length);
                offset += array.Length;
            }

            return data;
        }
    }
}

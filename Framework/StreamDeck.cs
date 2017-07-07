using System;
using System.Linq;
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

            keyState = new bool[numKeys];
            Read();
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

        public static void Dummy()
        {
            Console.WriteLine("Dummy");
        }
    }
}

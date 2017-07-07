using HidSharp;

namespace StreamDeck.Framework
{
    public static class StreamDeck
    {
        private const int vendorId = 0x0fd9;
        private const int productId = 0x0060;

        private static readonly HidDevice streamDeckDevice;

        static StreamDeck()
        {
            streamDeckDevice = FindUSBDevice();
        }

        public static HidDevice FindUSBDevice()
        {
            HidDeviceLoader loader = new HidDeviceLoader();
            HidDevice device = loader.GetDeviceOrDefault(vendorId, productId);
            if (device != null) return device;

            Logger.Log("StreamDeck", "Could not find StreamDeck USB device!", LoggerVerbosity.Error);
            return null;
        }
    }
}

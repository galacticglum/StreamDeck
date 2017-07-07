using System.Drawing;

namespace StreamDeck.Framework
{
    internal static class ImageUtilities
    {
        public static byte[] ToBytes(this Image image)
        {
            ImageConverter converter = new ImageConverter();
            return(byte[]) converter.ConvertTo(image, typeof(byte[]));
        }
    }
}

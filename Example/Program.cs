using StreamDeck.Framework;

namespace Example
{
    public class Program
    {
        private static void Main(string[] args)
        {
            StreamDeck.Framework.StreamDeck.KeyPressed += OnKeyPressed;
            //StreamDeck.Framework.StreamDeck.FillColour(0, new Colour(0, 255, 0));
            StreamDeck.Framework.StreamDeck.FillImage(0, "Data/Test.png");
        }

        private static void OnKeyPressed(KeyEventArgs args)
        {
            Logger.Log(args.KeyId);
        }
    }
}

using StreamDeck.Framework;

namespace Example
{
    public class Program
    {
        private static void Main(string[] args)
        {
            StreamDeck.Framework.StreamDeck.KeyPressed += OnKeyPressed;
            StreamDeck.Framework.StreamDeck.FillColour(0, new Colour(255, 0, 0));
        }

        private static void OnKeyPressed(KeyEventArgs args)
        {
            Logger.Log(args.KeyId);
        }
    }
}

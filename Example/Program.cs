using StreamDeck.Framework;

namespace Example
{
    public class Program
    {
        private static void Main(string[] args)
        {
            StreamDeck.Framework.StreamDeck.KeyPressed += OnKeyPressed;
        }

        private static void OnKeyPressed(KeyEventArgs args)
        {
            Logger.Log(args.KeyId);
        }
    }
}

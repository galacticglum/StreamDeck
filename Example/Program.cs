using System;
using StreamDeck.Framework;

namespace Example
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //StreamDeck.Framework.StreamDeck.FillColour(0, new Colour(0, 255, 0));
            //StreamDeck.Framework.StreamDeck.FillImage(0, "Data/Test.png");
            StreamDeck.Framework.StreamDeck.SetBrightness(100);
            StreamDeck.Framework.StreamDeck.RegisterKeyPressed(4, OnKey4Pressed);
        }

        private static void OnKey4Pressed(KeyEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("I PRESSED FOUR!");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}

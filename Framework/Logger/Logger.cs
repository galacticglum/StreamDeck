using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace StreamDeck.Framework
{
    public static class Logger
    {
        public const string AllCategoryVerbosities = "__ALL_CATEGORY_VERBOSITIES__";

        public static string LogFilePath { get; set; } = "Runtime.log";
        public static LoggerTimeStampMode TimeStampMode { get; set; } = LoggerTimeStampMode.DateTimeStamp;
        public static LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Info;
        public static string LineSeperator { get; set; } = string.Empty;

        /// <summary>
        /// The category verbosity filter. If set to null, then the filter will allow all categories.
        /// </summary>
        public static Dictionary<string, LoggerVerbosity> CategoryVerbosities { get; set; }
        private static readonly StringBuilder logBuffer = new StringBuilder();

        static Logger()
        {
            CategoryVerbosities = new Dictionary<string, LoggerVerbosity>
            {
                // Second parameter doesn't matter, if the key AllCategoryVerbosities is in the dictionary then it just logs any category.
                {AllCategoryVerbosities, LoggerVerbosity.Info}
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => WriteLogBufferToFile();
        }

        public static void Log(string category, string message, LoggerVerbosity messageVerbosity = LoggerVerbosity.Info, bool logUps = false, bool logFps = false)
        {
            lock (Console.Out)
            {
                if (Verbosity > messageVerbosity) return;
                if (CategoryVerbosities != null)
                {
                    if (!CategoryVerbosities.ContainsKey(category) && !CategoryVerbosities.ContainsKey(AllCategoryVerbosities)) return;
                    if (CategoryVerbosities.ContainsKey(category))
                    {
                        if (CategoryVerbosities[category] > messageVerbosity) return;
                    }
                }

                ConsoleColor oldConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = GetConsoleColour(messageVerbosity);

                string output = string.Concat(GetMessageHeader(category), message);
                logBuffer.AppendLine(string.Concat($"[{GetVerbosityName(messageVerbosity)}]", output));

                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(LineSeperator))
                {
                    Console.WriteLine(LineSeperator);
                }

                Console.ForegroundColor = oldConsoleColor;
            }
        }

        public static void Log(string message, LoggerVerbosity messageVerbosity = LoggerVerbosity.Info)
        {
            Log(string.Empty, message, messageVerbosity);
        }

        public static void WriteLogBufferToFile()
        {
            new Thread(() =>
            {
                lock (Console.Out)
                {
                    File.WriteAllText(LogFilePath, logBuffer.ToString());
                    logBuffer.Clear();
                }
            }).Start();
        }

        private static string GetMessageHeader(string category)
        {
            string dateTimeStamp = string.Empty;
            switch (TimeStampMode)
            {
                case LoggerTimeStampMode.TimeStamp:
                    dateTimeStamp = $"{DateTime.Now.ToShortTimeString()}";
                    break;
                case LoggerTimeStampMode.DateStamp:
                    dateTimeStamp = $"{DateTime.Now.ToShortDateString()}";
                    break;
                case LoggerTimeStampMode.DateTimeStamp:
                    dateTimeStamp = $"{DateTime.Now}";
                    break;
            }

            string headerContents = !string.IsNullOrEmpty(dateTimeStamp) ? $"{dateTimeStamp}" : string.Empty;
            string header = !string.IsNullOrEmpty(headerContents) ? $"[{headerContents}] " : string.Empty;
            string categoryHeader = !string.IsNullOrEmpty(category) ? $"{category}: " : string.Empty;
            return $"{header}{categoryHeader}";
        }

        private static ConsoleColor GetConsoleColour(LoggerVerbosity verbosity)
        {
            switch (verbosity)
            {
                case LoggerVerbosity.Info:
                    return ConsoleColor.White;
                case LoggerVerbosity.Warning:
                    return ConsoleColor.Yellow;
                case LoggerVerbosity.Error:
                    return ConsoleColor.Red;
            }

            return ConsoleColor.Gray;
        }

        private static string GetVerbosityName(LoggerVerbosity verbosity)
        {
            switch (verbosity)
            {
                case LoggerVerbosity.Info:
                    return "Info";
                case LoggerVerbosity.Warning:
                    return "Warning";
                case LoggerVerbosity.Error:
                    return "Error";
            }

            return string.Empty;
        }
    }
}
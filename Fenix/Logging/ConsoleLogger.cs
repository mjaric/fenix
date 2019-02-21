using System;

namespace Fenix.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly LogLevel _logLevel;

        public ConsoleLogger(LogLevel logLevel = LogLevel.Debug)
        {
            _logLevel = logLevel;
        }

        public void Debug(string message)
        {
            if (_logLevel >= LogLevel.Debug)
                Log("debug", message);
        }

        public void Info(string message)
        {
            if (_logLevel >= LogLevel.Info)
                Log("info", message);
        }

        public void Warn(string message)
        {
            if (_logLevel >= LogLevel.Warn)
                Log("warn", message);
        }


        public void Error(string message)
        {
            if (_logLevel >= LogLevel.Error)
                Log("error", message);
        }

        private void Log(string level, string message)
        {
            var fgColor = Console.ForegroundColor;
            switch (level)
            {
                case "debug":
                    Console.ForegroundColor = Console.BackgroundColor == ConsoleColor.White
                            ? ConsoleColor.DarkCyan
                            : ConsoleColor.Cyan;
                    break;
                case "info":
                    Console.ForegroundColor = Console.BackgroundColor == ConsoleColor.White
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.Gray;
                    break;
                case "warn":
                    Console.ForegroundColor = Console.BackgroundColor == ConsoleColor.White
                            ? ConsoleColor.DarkYellow
                            : ConsoleColor.Yellow;
                    break;

                case "error":
                    Console.ForegroundColor = Console.BackgroundColor == ConsoleColor.White
                            ? ConsoleColor.DarkRed
                            : ConsoleColor.Red;
                    break;
            }

            Console.WriteLine($"[{level.ToUpper()}] [{DateTime.Now:u}] {message}");
            Console.ForegroundColor = fgColor;
        }
    }
}
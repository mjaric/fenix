using System;

namespace Fenix.Logging
{
    public class NoopLogger : ILogger
    {
        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(Exception ex)
        {
        }

        public static readonly ILogger Instance = new NoopLogger();
    }
}
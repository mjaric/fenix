using System;

namespace Fenix.Common
{
    static class Ensure
    {
        public static void NotNull<T>(T argument, string argumentName) where T : class 
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void NotNullOrEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentNullException(argument, argumentName);
        }

        public static void Positive(int number, string argumentName)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be positive.");
        }

        public static void Positive(long number, string argumentName)
        {
            if (number <= 0L)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be positive.");
        }
        
        public static void Positive(double number, string argumentName)
        {
            if (number <= 0D)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be positive.");
        }

        public static void Nonnegative(long number, string argumentName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be non-negative.");
        }

        public static void Nonnegative(int number, string argumentName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be non-negative.");
        }

        public static void NotEmptyGuid(Guid guid, string argumentName)
        {
            if (Guid.Empty == guid)
                throw new ArgumentException(argumentName, argumentName + " should be non-empty GUID.");
        }

        public static void Equal(int expected, int actual)
        {
            if (expected != actual)
                throw new Exception($"expected {expected} actual {actual}");
        }
    }
}
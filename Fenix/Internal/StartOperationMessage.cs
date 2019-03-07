using System;
using Fenix.ClientOperations;
using Fenix.Common;

namespace Fenix.Internal
{
    internal class StartOperationMessage : InternalMessage
    {
        public readonly IClientOperation Operation;
        public readonly int MaxRetries;
        public readonly TimeSpan Timeout;

        public StartOperationMessage(IClientOperation operation, int maxRetries, TimeSpan timeout)
        {
            Ensure.NotNull(operation, "operation");
            Operation = operation;
            MaxRetries = maxRetries;
            Timeout = timeout;
        }
    }
}
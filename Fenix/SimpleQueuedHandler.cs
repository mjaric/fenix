using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Fenix.Common;

namespace Fenix
{
    internal class SimpleQueuedHandler
    {
        private readonly ConcurrentQueue<InternalMessage> _messageQueue = new ConcurrentQueue<InternalMessage>();

        private readonly Dictionary<Type, Action<InternalMessage>> _handlers =
                new Dictionary<Type, Action<InternalMessage>>();

        private int _isProcessing;

        public SimpleQueuedHandler RegisterHandler<T>(Action<T> handler) where T : InternalMessage
        {
            Ensure.NotNull(handler, nameof(handler));
            _handlers.Add(typeof(T), msg => handler((T) msg));
            return this;
        }

        public void EnqueueMessage(InternalMessage message)
        {
            Ensure.NotNull(message, nameof(message));
            _messageQueue.Enqueue(message);
            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
            {
                ThreadPool.QueueUserWorkItem(ProcessQueue);
            }
        }

        private void ProcessQueue(object state)
        {
            do
            {
                while (_messageQueue.TryDequeue(out var message))
                {
                    if (!_handlers.TryGetValue(message.GetType(), out var handler))
                    {
                        throw new Exception($"No handler registered for message {message.GetType().Name}");
                    }

                    handler(message);
                }

                Interlocked.Exchange(ref _isProcessing, 0);
            } while (_messageQueue.Count > 0 && Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0);
        }
    }
}
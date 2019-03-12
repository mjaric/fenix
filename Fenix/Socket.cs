using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Fenix.ClientOperations;
using Fenix.Internal;
using Newtonsoft.Json.Linq;

namespace Fenix
{
    public class Socket : IDisposable
    {
        private SocketLogicHandler _handler;

        public Settings Settings { get; }

        public Socket(Settings settings)
        {
            Settings = settings;

            _handler = new SocketLogicHandler(this, settings);
        }

        /// <summary>
        /// Asynchronously connects to phoenix web socket.
        /// </summary>
        /// <param name="uri">Uri at which connection should be established</param>
        /// <param name="parameters">Parameters that should be append to query string</param>
        /// <returns>Task</returns>
        public Task ConnectAsync(Uri uri, (string, string)[] parameters = null)
        {
            parameters = parameters ?? new (string, string)[] { };
            var source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handler.EnqueueMessage(new StartConnectionMessage(source, uri, parameters));
            return source.Task;
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public void Close()
        {
            _handler.EnqueueMessage(new CloseConnectionMessage(WebSocketCloseStatus.NormalClosure,
                "Connection close requested by client.", null));
        }

        public IChannel Channel(string topic, object payload)
        {
            return _handler.SubscribeTopic(topic, payload);
        }

        private async Task EnqueueOperation(IClientOperation operation)
        {
            while (_handler.TotalOperationCount >= Settings.MaxQueueSize)
            {
                await Task.Delay(1).ConfigureAwait(false);
            }

            _handler.EnqueueMessage(
                new StartOperationMessage(operation, Settings.MaxRetries, Settings.OperationTimeout));
        }
        
        public event EventHandler<ClientConnectionEventArgs> Connected
        {
            add => _handler.Connected += value;
            remove => _handler.Connected -= value;
        }

        public event EventHandler<ClientConnectionEventArgs> Disconnected
        {
            add => _handler.Disconnected += value;
            remove => _handler.Disconnected -= value;
        }

        public event EventHandler<ClientReconnectingEventArgs> Reconnecting
        {
            add => _handler.Reconnecting += value;
            remove => _handler.Reconnecting -= value;
        }

        public event EventHandler<ClientClosedEventArgs> Closed
        {
            add => _handler.Closed += value;
            remove => _handler.Closed -= value;
        }

        public event EventHandler<ClientErrorEventArgs> ErrorOccurred
        {
            add => _handler.ErrorOccurred += value;
            remove => _handler.ErrorOccurred -= value;
        }

    }
}
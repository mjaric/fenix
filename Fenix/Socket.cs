using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Fenix.Internal;

namespace Fenix
{
    public class Socket : IDisposable
    {
        private SocketLogicHandler _handler;

        public Options ConnectionOptions { get; }

        public Socket(Options options)
        {
            ConnectionOptions = options;

            _handler = new SocketLogicHandler(this, options);
        }

        /// <summary>
        /// Asynchronously connects to phoenix web socket.
        /// </summary>
        /// <param name="uri">Uri at which connection should be established</param>
        /// <param name="parameters">Parameters that should be append to query string</param>
        /// <returns>Task</returns>
        public Task ConnectAsync(Uri uri, (string, string)[] parameters = null)
        {
            parameters = parameters ?? new (string, string)[]{};
            var source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handler.EnqueueMessage(new EstablishConnectionMessage(source, uri, parameters));
            return source.Task;
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public void Close()
        {
            _handler.EnqueueMessage(new CloseConnectionMessage(WebSocketCloseStatus.NormalClosure, "Connection close requested by client.", null));
        }

        /// <summary>
        /// Joins to phoenix channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="parameters"></param>
        /// <param name="onPush"></param>
        /// <param name="onLeave"></param>
        /// <returns></returns>
        public Task<Channel> JoinChannelAsync(
            string channelName, 
            object parameters, 
            Action<Channel, Push> onPush,
            Action<Channel, ChannelLeaveReason, Exception> onLeave = null
        )
        {
            var source = new TaskCompletionSource<Channel>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handler.EnqueueMessage(new JoinChannelMessage(
                source,
                channelName,
                parameters,
                ConnectionOptions.PushTimeout,
                onPush,
                onLeave
            ));
            return source.Task;
        }
    }
}
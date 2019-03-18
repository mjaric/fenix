using System;
using System.Collections.Concurrent;
using Fenix.Exceptions;

namespace Fenix.Internal
{
    internal class ChannelManager
    {
        private Settings _settings;
        private readonly SocketLogicHandler _handler;

        private ConcurrentDictionary<string, Channel> _channels =
                new ConcurrentDictionary<string, Channel>();


        public ChannelManager(Settings settings, SocketLogicHandler handler)
        {
            _settings = settings;
            _handler = handler;
        }


        public void PurgeSubscribedAndDroppedChannels(Guid connectionId)
        {
            // only unsubscribe channels, will be subscribed again on join
            foreach (var channel in _channels.Values)
            {
                channel.Close(false);
            }
        }

        public Channel CreateChannel(string topic, object payload)
        {
            var channel = new Channel(_settings, _handler, topic, payload);
            return _channels.AddOrUpdate(topic, channel, (t, oldChannel) =>
            {
                oldChannel.LeaveAsync().ConfigureAwait(false);
                return channel;
            });
        }

        public bool RemoveChannel(string topic, Channel channel)
        {
            if (!_channels.TryGetValue(topic, out var aChannel))
                return true;
            return aChannel.JoinRef != channel.JoinRef || _channels.TryRemove(topic, out _);
        }

        public void RejoinChannels(WebSocketConnection connection)
        {
            foreach (var channel in _channels.Values)
            {
                if (!channel.Joined)
                {
                    channel.JoinAsync().ConfigureAwait(false);
                }
            }
        }

        public void CleanUp()
        {
            var connectionClosedException =
                    new ConnectionClosedException("Connection was closed.");
            foreach (var channel in _channels.Values)
            {
                channel.Fail(connectionClosedException);
            }

            _channels.Clear();
        }

        public Channel AddOrUpdate(string topic, Channel channel, Func<string, Channel, Channel> replacement)
        {
            return _channels.AddOrUpdate(topic, channel, replacement);
        }

        public bool TryGetValue(string topic, out Channel channel)
        {
            return _channels.TryGetValue(topic, out channel);
        }

        public bool TryRemove(string topic, out Channel channel)
        {
            return _channels.TryRemove(topic, out channel);
        }
    }
}
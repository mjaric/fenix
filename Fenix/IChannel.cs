using System;
using System.Threading.Tasks;
using Fenix.Responses;
using Newtonsoft.Json.Linq;

namespace Fenix
{
    /// <summary>
    /// Phoenix channel
    /// </summary>
    public interface IChannel
    {   
        /// <summary>
        /// Channels topic name
        /// </summary>
        string Topic { get; }

        /// <summary>
        /// Subscribes channel to event with given callback
        /// </summary>
        /// <param name="eventName">Event to which channel should subscribe callback.</param>
        /// <param name="callback">Method/Callback that will be executed when event appear.</param>
        IChannel Subscribe(string eventName, Action<IChannel, JObject> callback);

        /// <summary>
        /// Unsubscribe any callbacks that are previously subscribed to specified event.
        /// </summary>
        /// <param name="eventName">Name of the event that caller wants to unsubscribe.</param>
        IChannel Unsubscribe(string eventName);

        /// <summary>
        /// Initiates async join operation. Once "phx_reply" is received, channel will be ready to send and receive messages.
        /// </summary>
        /// <returns>
        /// <see cref="JoinResult"/> if operation finishes with "phx_reply" response from server.
        /// Please note that operation can result with status equal to "ok" or "error"!
        /// </returns>
        Task<JoinResult> JoinAsync();

        /// <summary>
        /// When called, leaves channel and stops all subscriptions.
        /// </summary>
        void Leave();

        /// <summary>
        /// Pushes payload to socket
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="payload">The payload that should be sent to socket</param>
        /// <param name="maxRetries">Number of retries before operation is considered as failed.</param>
        /// <param name="timeout">TimeSpan after which operation will timout if no reply is received from server.</param>
        /// <returns></returns>
        Task<SendResult> SendAsync(string eventType, object payload, int? maxRetries = null, TimeSpan? timeout = null);
    }
}
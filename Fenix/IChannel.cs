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
        /// Initiates async con operation.
        /// </summary>
        /// <param name="payload">optional join payload</param>
        /// <returns></returns>
        Task<JoinResult> JoinAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task LeaveAsync();

        /// <summary>
        /// Pushes payload to socket
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="payload">The payload that should be sent to socket</param>
        /// <returns></returns>
        Task<SendResult> SendAsync(string eventType, object payload);
    }
}
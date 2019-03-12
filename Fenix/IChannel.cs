using System;
using System.Threading.Tasks;
using Fenix.Exceptions;
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
        /// Subscribes channel to event with given callback.
        /// </summary>
        /// <param name="eventName">Event to which channel should subscribe callback</param>
        /// <param name="callback">Method/Callback that will be executed when event appear</param>
        IChannel Subscribe(string eventName, Action<IChannel, JObject> callback);

        /// <summary>
        /// Unsubscribe any callbacks that are previously subscribed to specified event.
        /// </summary>
        /// <param name="eventName">Name of the event that caller wants to unsubscribe.</param>
        IChannel Unsubscribe(string eventName);

        /// <summary>
        /// Initiates Async join operation.
        ///
        /// On successful execution, task will yield <see cref="PushResult"/> and application must inspect and check if replied <seealso cref="PushResult.Status"/>
        /// is equal to "ok" or "error". In case of "error" channel will be automatically closed! If status is "ok" then
        /// any server push will be channeled to this channel.
        ///
        /// Please note, only one channel per topic can be opened at the time. If you try to open second channel
        /// with same topic as first one, the first one will be closed automatically.
        ///
        /// This push operation never timout and it will retry always to join channel until it succeed.
        /// </summary>
        /// <returns><see cref="PushResult"/> representing result</returns>
        /// <exception cref="JoinFailedException"></exception>
        Task<PushResult> JoinAsync();

        /// <summary>
        /// Asynchronously sends `phx_leave` event to server for channel and closes it. All event subscriptions will be
        /// unsubscribed automatically.
        /// </summary>
        /// <param name="timeout">Time to wait operation to complete, if time elapse, operation will be considered
        /// timeout and exception will be thrown.</param>
        /// <returns><see cref="PushResult"/> representing reply from server</returns>
        /// <exception cref="OperationTimedOutException">Indicates that timout is reached and operation failed.</exception>
        Task<PushResult> LeaveAsync(TimeSpan? timeout = null);

        /// <summary>
        /// Pushes event to server
        /// </summary>
        /// <param name="eventType">Event name/type</param>
        /// <param name="payload">Payload that should be attached to event</param>
        /// <param name="maxRetries">Number of retries if operation fail</param>
        /// <param name="timeout">Time after which operation is canceled</param>
        /// <returns><see cref="PushResult"/> representing reply from server for this push</returns>
        /// <exception cref="PushFailException"></exception>
        /// <exception cref="OperationTimedOutException">Indicates that timout is reached and operation failed.</exception>
        Task<PushResult> SendAsync(string eventType, object payload, int? maxRetries = null, TimeSpan? timeout = null);
    }
}
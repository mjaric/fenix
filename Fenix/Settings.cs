using System;
using Fenix.Logging;

namespace Fenix
{
    /// <summary>
    /// Options for Fenix <see cref="Socket"/>
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Gets or sets the timeout that will be triggered if push response 
        /// is not received in given time frame.
        /// 
        /// Default timeout us 10 seconds.
        /// </summary>
        /// <value>The timeout.</value>
        public TimeSpan PushTimeout { get; set; }

        /// <summary>
        /// Gets or sets the channel rejoin interval, the interval in which 
        /// errored channel will be rejoined. 
        /// 
        /// Default setting is 1 second, and when set to null it will never 
        /// attempt to rejoin.
        /// </summary>
        /// <value>The channel rejoin timeout.</value>
        public TimeSpan ChannelRejoinTimeout { get; set; }

        /// <summary>
        /// The interval at which to send heartbeat messages.
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; }

        /// <summary>
        /// The interval after which an unacknowledged heartbeat will cause the connection to be considered faulted and disconnect.
        /// </summary>
        public TimeSpan HeartbeatTimeout { get; set; }

        /// <summary>
        /// Logger used to log connection events to log.
        ///
        /// Default Debug logger is <see cref="ConsoleLogger"/> and in any other profile <see cref="NoopLogger"/>.
        /// </summary>
        public ILogger Logger { get; set; }


        /// <summary>
        /// The amount of time to delay before attempting to reconnect.
        /// 
        /// Default timeout is 5 seconds.
        /// </summary>
        public TimeSpan ReconnectionDelay { get; set; }

        /// <summary>
        /// The amount of time a request for an operation is permitted to be queued awaiting transmission to the server.
        /// 
        /// Default is unlimited.
        /// </summary>
        public TimeSpan QueueTimeout { get; set; }

        /// <summary>
        /// The amount of time before an operation is considered to have timed out.
        /// 
        /// Default is 7 seconds.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// The amount of time that timeouts are checked in the system.
        /// 
        /// Default is 1 second.
        /// </summary>
        public TimeSpan OperationTimeoutCheckPeriod { get; set; }
        
        /// <summary>
        /// Whether to raise an error if no response is received from the server for an operation.
        /// 
        /// Default is false.
        /// </summary>
        public bool FailOnNoServerResponse { get; set; }

        /// <summary>
        /// Maximum number of concurrent operations/pushes that can be run at the same time.
        ///
        /// Default value is 5000.
        /// </summary>
        public int MaxConcurrentItems { get; set; }
        
        /// <summary>
        /// Maximum number of reconnect attempts when endpoint is unreachable or disconnected.
        ///
        /// Default is Int32 max value.
        /// </summary>
        public int MaxReconnections { get; set; }
        
        /// <summary>
        /// The maximum number of retry attempts.
        /// Default 2
        /// </summary>
        public int MaxRetries { get; set; }
        /// <summary>
        /// The maximum number of outstanding items allowed in the queue.
        ///
        /// Default is 5000
        /// </summary>
        public int MaxQueueSize { get; set; }
        
        public Settings()
        {
            PushTimeout = TimeSpan.FromSeconds(10);
            ChannelRejoinTimeout = TimeSpan.FromSeconds(1);
#if DEBUG
            Logger = new ConsoleLogger();
            #else
            Logger = new NoopLogger();
#endif
            

            ReconnectionDelay = Consts.DefaultReconnectionDelay;
            QueueTimeout = Consts.DefaultQueueTimeout;
            OperationTimeout = Consts.DefaultOperationTimeout;
            OperationTimeoutCheckPeriod = Consts.DefaultOperationTimeoutCheckPeriod;
            
            HeartbeatInterval = TimeSpan.FromSeconds(10);
            HeartbeatTimeout = TimeSpan.FromSeconds(4);

            FailOnNoServerResponse = false;
            MaxConcurrentItems = 5000;
            
            MaxReconnections = Int32.MaxValue;

            MaxRetries = 2;
            MaxQueueSize = 5000;

        }
    }
}
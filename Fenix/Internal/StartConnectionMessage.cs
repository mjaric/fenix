using System;
using System.Threading.Tasks;

namespace Fenix.Internal
{
    internal class StartConnectionMessage : InternalMessage
    {
        public readonly TaskCompletionSource<object> Source;
        public readonly Uri Uri;
        public readonly (string, string)[] Parameters;

        public StartConnectionMessage(
            TaskCompletionSource<object> source, 
            Uri uri, 
            (string, string)[] parameters
        )
        {
            Source = source;
            Uri = uri;
            Parameters = parameters;
        }
    }

    internal class ConnectionEstablishedMessage : InternalMessage
    {
        public readonly WebSocketConnection Connection;

        public ConnectionEstablishedMessage(WebSocketConnection connection)
        {
            Connection = connection;
        }
    }
}
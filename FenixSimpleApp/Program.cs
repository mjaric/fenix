using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fenix;
using Fenix.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FenixSimpleApp
{
    class Program
    {
        private const string token =
                "SFMyNTY.g3QAAAACZAAEZGF0YW0AAAAEMTIzNGQABnNpZ25lZG4GAJtqq_JoAQ.UtOuMffuGwSHPUWGcJVrwYq4xoT8Ficssni_BzZh0rk";

        private Socket _socket;

        public Program()
        {
            _socket = new Socket(new Options());
        }

        static void Main(string[] args)
        {
            var app = new Program();

            // test concurrent connecting, should throw error on second attempt
            Task.Run(() => app.Connect());
            Task.Run(() => app.Connect());

            Task.Delay(TimeSpan.FromSeconds(3))
                .ContinueWith(t => { app.JoinLobby(); });


            // multiple independent connections initiated from multiple tasks
            // ConnectMultipleTimes();


            Console.ReadLine();
        }

        private static void ConnectMultipleTimes()
        {
            var cts = new CancellationTokenSource(10000); // Auto-cancel after 5 sec
            Action<Task> repeatAction = null;
            repeatAction = _ignored1 =>
            {
                var app2 = new Program();
                app2.Connect();
                Task.Delay(1000, cts.Token)
                    .ContinueWith(_ignored2 => repeatAction(_ignored2), cts.Token); // Repeat every 1 sec
            };
            Task.Delay(2000, cts.Token).ContinueWith(repeatAction, cts.Token); // Launch with 2 sec delay
            cts.Token.WaitHandle.WaitOne();
        }


        private async void Connect()
        {
            try
            {
                var uri = new Uri("ws://localhost:4000/socket/websocket");
                await _socket.ConnectAsync(uri, new[] {("token", token)});
                Console.WriteLine("Connected!!!!!!!!!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: [{ex.GetType().FullName}] \"{ex.Message}\"");
            }
        }


        private void JoinLobby()
        {
            /*var channel = _socket.CreateChannel("room:lobby", new[] {("nick_name", "Test User")});

            channel.On("new_message", (channel, payload) =>
            {
                Console.WriteLine($@"
                Got LOBBY message
                {payload.Value<string>("body")}
                ");
            });


            channel.JoinAsync()
                   .ContinueWith(t2 =>
                   {
                       if (!t2.IsFaulted)
                       {
                           channel.PushAsync(new ChatMessage("Hello there!"))
                                  .ContinueWith(t =>
                                  {
                                      Console.WriteLine(t.IsFaulted
                                              ? $"FAILED to send message to lobby [{t.Exception.Message}, {t.Exception.GetType()}]"
                                              : "Message sent!!!!!");
                                  });
                       }
                       else
                       {
                           Console.WriteLine(
                               $"FAILED to send message to lobby [{t2.Exception.Message}, {t2.Exception.GetType()}]");
                       }
                   });*/
        }

        private void OnLobbyLeave(Channel channel, ChannelLeaveReason reason, Exception ex)
        {
            _socket.ConnectionOptions.Logger.Info(
                $"Channel Leave: {channel.Topic}, reason {reason}, exception [{ex?.GetType()}, {ex?.Message}] ");
        }

        private void OnLobbyMessage(Channel channel, Push push)
        {
            throw new NotImplementedException();
        }

        private class ChatMessage
        {
            public string Body { get; set; }

            public ChatMessage(string body)
            {
                Body = body;
            }
        }
    }
}
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
                "SFMyNTY.g3QAAAACZAAEZGF0YW0AAAAEMTIzNGQABnNpZ25lZG4GAJMq6E5pAQ.fiAuw0yCDMvzi_gCkAbcWxAqTnWDW6w5yod8-RlQJME";

        private Socket _socket;

        public Program()
        {
            var settings = new Settings();
            _socket = new Socket(settings);
            _socket.Connected += (sender, args) =>
            {
                settings.Logger.Info("CONNECTED");
            };
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


        private async void JoinLobby()
        {
            var channel = _socket.Channel("room:lobby", new {NickName = "Timotije"});

            channel.Subscribe("new_msg", (ch, payload) =>
            {
                Console.WriteLine($@"
                Got LOBBY message
                {payload.Value<string>("body")}
                ");
            });


            try
            {
                var result = await channel.JoinAsync();
                _socket.Settings.Logger.Info($"JOINED: {result.Response}");
                Task.Delay(5000).ContinueWith(task => { channel.SendAsync("new_msg", new {body = "Hi guys 1"}); });
                Task.Delay(5000).ContinueWith(task => { channel.SendAsync("new_msg", new {body = "Hi guys 2"}); });
                Task.Delay(5000).ContinueWith(task => { channel.SendAsync("new_msg", new {body = "Hi guys 3"}); });
//                await channel.SendAsync("new_msg", new {body = "Hi there"});

            }
            catch (Exception ex)
            {
                _socket.Settings.Logger.Error(ex);
            }
        }

        private void OnLobbyLeave(Channel channel, ChannelLeaveReason reason, Exception ex)
        {
            _socket.Settings.Logger.Info(
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
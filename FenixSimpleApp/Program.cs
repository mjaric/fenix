using System;
using System.Collections.Generic;
using System.Linq;
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
                "SFMyNTY.g3QAAAACZAAEZGF0YW0AAAAEMTIzNGQABnNpZ25lZG4GADXlTPJpAQ.L0TvyizjtH-piNXJu8y_FKIrXjS7zHR2uoFLsv-Otqs";

        private Socket _socket;

        public Program()
        {
            var settings = new Settings()
            {
                HeartbeatTimeout = TimeSpan.FromSeconds(1)
            };
            _socket = new Socket(settings);
            _socket.Connected += (sender, args) => { settings.Logger.Info("CONNECTED"); };
        }

        static void Main(string[] args)
        {
            var app = new Program();

            // test concurrent connecting, should throw error on second attempt
            Task.Run(() => app.Connect());
//            Task.Run(() => app.Connect());

            Task.Delay(TimeSpan.FromSeconds(3))
                .ContinueWith(t => { app.JoinLobby(); });


            // multiple independent connections initiated from multiple tasks
            // ConnectMultipleTimes();


            Console.ReadLine();
        }

        private static void ConnectMultipleTimes()
        {
            var cts = new CancellationTokenSource(10000); // Auto-cancel after 5 sec

            void RepeatAction(Task _)
            {
                var app2 = new Program();
                app2.Connect();
                Task.Delay(1000, cts.Token).ContinueWith(RepeatAction, cts.Token); // Repeat every 1 sec
            }

            Task.Delay(2000, cts.Token).ContinueWith(RepeatAction, cts.Token); // Launch with 2 sec delay
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

            channel.Subscribe(ChannelEvents.Close,
                (ch, response) => { Console.WriteLine($"Channel {channel.Topic} is closed now!"); });

            channel.Subscribe("server_time",
                (ch, response) => { Console.WriteLine($"Server time {response.Value<string>("message")}!"); });


            try
            {
                var result = await channel.JoinAsync();
                _socket.Settings.Logger.Info($"JOIN COMPLETED: status = {result.Status}, response: {result.Response}");


                // concurrent send
                foreach (var thread in Enumerable.Range(1, 3))
                {
                    _ = Task.Delay(500)
                        .ContinueWith(async task =>
                        {
                            long no = 1;
                            while (no < 100)
                            {
                                await channel.SendAsync("new_msg", new { body = $"[{thread}] Hi guys #{no++}" });
                            }
                        });
                }

                _ = Task.Delay(10000).ContinueWith(task => { _socket.Close(); }).ConfigureAwait(false);
                await channel.SendAsync("new_msg", new {body = "Hi guys 1"});
            }
            catch (Exception ex)
            {
                _socket.Settings.Logger.Error(ex);
            }
        }
    }
}
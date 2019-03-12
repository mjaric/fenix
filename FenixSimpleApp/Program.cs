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

            channel.Subscribe(ChannelEvents.Close, (ch, response) =>
            {
                Console.WriteLine($"Channel {channel.Topic} is closed now!");
            });
            
            
            try
            {
                var result = await channel.JoinAsync();
                _socket.Settings.Logger.Info($"JOIN COMPLETED: status = {result.Status}, response: {result.Response}");
                

                Task.Delay(1000).ContinueWith(async task =>
                {
                    await channel.SendAsync("new_msg", new {body = "Hi guys 2"});
                }).ConfigureAwait(false);
                Task.Delay(2000).ContinueWith(async task =>
                {
                    await channel.SendAsync("new_msg", new {body = "Hi guys 3"});
                }).ConfigureAwait(false);
                Task.Delay(1500).ContinueWith(async task => { await channel.LeaveAsync(); }).ConfigureAwait(false);
                await channel.SendAsync("new_msg", new {body = "Hi guys 1"});
            }
            catch (Exception ex)
            {
                _socket.Settings.Logger.Error(ex);
            }
        }
    }
}
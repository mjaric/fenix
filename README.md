# Fenix
Thread safe .NET Standard 2 implementation of Phoenix framework websocket protocol.

## Supported/Tested .NET Runtime

- NETCore 2.1.0
- Mono 5.16.0.221
- .NET Framework 4.6 and above

## Dependencies:
- Newtonsoft.Json 12.0.1

## Usage

#### Add Dependency to Fenix NuGet Package
Add Fenix nuget dependency either using your IDEs Package Manager tool or using command line:

```
$ dotnet add package Fenix
```
or amend your csproj/vbproj by adding:
```
<PackageReference Include="Fenix" Version="0.1.*" />
```
and then build your project for example:
```
$ dotnet build MyProject.csproj
```

#### Create Fenix Socket

```c#
use Fenix;


// some constants
const token = "dsa90disadojsaoijadoiajsodiajs";

// Defaults should be ok, but you can tweak some options, e.g. tur on logging, max retries etc.. 
var settings = new Settings();
var socket = new Socket(settings);
try
{
    var uri = new Uri("ws://localhost:4000/socket/websocket");
    // "token" is not required, but below is demo how to pass parameteters while connecting
    await _socket.ConnectAsync(uri, new[] {("token", token)});
    
    var channel = _socket.Channel("room:lobby", new {NickName = "Timotije"});
    channel.Subscribe("new_msg", (ch, payload) =>
    {
        Console.WriteLine($@"
        Got LOBBY message
        {payload.Value<string>("body")}
        ");
    });
    
    var result = await channel.JoinAsync();
    Console.WriteLine($"Lobby JOIN: status = '{result.Status}', response: {result.Response}");
    
    await channel.SendAsync("new_msg", new {body = "Hi guys 2"});
    
    Task.Delay(10000)
        .ContinueWith(async task => { await channel.LeaveAsync(); })
        .ConfigureAwait(false);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: [{ex.GetType().FullName}] \"{ex.Message}\"");
}
```

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

```
//
var settings = new Fenix.Settings();
```

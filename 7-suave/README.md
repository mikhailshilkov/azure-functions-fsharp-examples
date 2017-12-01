# F# HTTP Trigger Functions for .NET Core 2.0

Examples of Azure Functions with HTTP trigger, implemented in F# and running on .NET Core 2.0 with Azure Functions 2.0 runtime (currently in beta/preview).

## Running examples

``` bash
dotnet restore
dotnet build
dotnet publish
cd bin\debug\netstandard2.0\publish
func host start
```

or just execute `run` command from Visual Studio Code.

## Simple HTTP

Minimal example of HTTP Trigger in F#

Sample URL: http://localhost:7071/api/simple?name=User

Response: `Hi, User`

## Suave HTTP

Example using [Suave](https://suave.io/) F#-friendly web library

Sample URL: http://localhost:7071/api/suave/goodbye

Response: `Good bye GET`

Based on [Tamizhvendan](https://github.com/tamizhvendan)'s [`Suave.Azure.Functions`](https://github.com/tamizhvendan/Suave.Azure.Functions)

## Other Links

Blog post [Develop Azure Functions on any platform](http://blogs.msdn.microsoft.com/appserviceteam/2017/09/25/develop-azure-functions-on-any-platform/) 

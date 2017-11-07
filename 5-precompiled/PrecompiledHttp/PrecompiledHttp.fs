namespace PrecompiledApp

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs.Host

module PrecompiledHttp =
  let run(req: HttpRequest, log: TraceWriter) =
    log.Info("F# HTTP trigger function processed a request.")

    ContentResult(Content = "Boom! Precompiled .NET Core function works!", ContentType = "text/html")

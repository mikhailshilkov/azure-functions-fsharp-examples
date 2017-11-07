namespace AttributeBasedApp

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host

module AttributeBased =

  [<FunctionName("AttributeBased")>]
  let run([<HttpTrigger>] req: HttpRequest, log: TraceWriter) =
    log.Info("F# HTTP trigger function processed a request.")

    ContentResult(Content = "function.json was successfully auto-generated.", ContentType = "text/html")

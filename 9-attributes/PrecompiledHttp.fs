namespace AttributeBasedApp

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs.Extensions.Http

module AttributeBased =

  [<FunctionName("AttributeBased")>]
  let run([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hellosanta")>] req: HttpRequest, log: TraceWriter, context: ExecutionContext) =
    log.Info("F# HTTP trigger function processed a request.")
    log.Info context.FunctionDirectory

    ContentResult(Content = "function.json was successfully auto-generated.", ContentType = "text/html")

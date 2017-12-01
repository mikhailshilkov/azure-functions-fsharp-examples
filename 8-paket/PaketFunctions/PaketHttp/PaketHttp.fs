namespace PaketFunctions

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs.Host

module PaketHttp =
  let run(req: HttpRequest, log: TraceWriter) =
    log.Info("F# HTTP trigger function processed a request.")

    ContentResult(Content = "You can use Paket in Function Apps", ContentType = "text/html")

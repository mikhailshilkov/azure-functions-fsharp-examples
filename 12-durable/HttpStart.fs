namespace DurableFsharp

open System
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs.Extensions.Http

module HttpStart = 

  [<FunctionName("HttpStart")>]
  let Run([<HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/{functionName}")>] req: HttpRequestMessage,
          [<OrchestrationClient>] starter: DurableOrchestrationClient,
          functionName: string,
          log: TraceWriter) =
    async {
      let! eventData =  req.Content.ReadAsAsync<Object>() |> Async.AwaitTask
      let! instanceId = starter.StartNewAsync(functionName, eventData) |> Async.AwaitTask

      log.Info(sprintf "Started orchestration with ID = '{%s}'." instanceId)

      let res = starter.CreateCheckStatusResponse(req, instanceId)
      res.Headers.RetryAfter <- RetryConditionHeaderValue(TimeSpan.FromSeconds(10.0))
      return res
    } |> Async.StartAsTask
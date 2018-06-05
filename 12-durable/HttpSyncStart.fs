namespace DurableFsharp

open System
open System.Net.Http
//open System.Net.Http.Headers
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Azure.WebJobs.Host
open FSharp.Control.Tasks

module HttpSyncStart = 

  // let Timeout = "timeout"
  // let RetryInterval = "retryInterval"

//   let GetTimeSpan(request: HttpRequestMessage, queryParameterName: string): Nullable<TimeSpan> =
//     let queryParameterStringValue = request.GetQueryNameValuePairs()
// //        .FirstOrDefault(x => x.Key == queryParameterName)
// //        .Value;
//     if String.IsNullOrEmpty(queryParameterStringValue) then Nullable()
//     else Nullable()

  [<FunctionName("HttpSyncStart")>]
  let Run([<HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/{functionName}/wait")>] req: HttpRequestMessage,
          [<OrchestrationClient>] starter: DurableOrchestrationClient,
          functionName: string,
          log: TraceWriter) =
    task {
      let! eventData =  req.Content.ReadAsAsync<Object>()
      let! instanceId = starter.StartNewAsync(functionName, eventData)

      log.Info(sprintf "Started orchestration with ID = '{%s}'." instanceId)

      return! starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, Nullable(), Nullable())
    }
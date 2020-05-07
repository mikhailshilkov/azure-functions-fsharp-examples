namespace Azure.Messaging.Demo.Server

open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Azure.ServiceBus
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Azure.WebJobs.ServiceBus
open Newtonsoft.Json
open System.IO
open System.Text

[<CLIMutable>]
type MyCustomType = {
    SessionId: string
    EventId: int }

module FromHttpTriggerToServiceBusTopic =
    [<Literal>]
    let MessageContentType = "application/json"
    [<Literal>]
    let TopicName = "my-topic-name"
    // See usage in the ServiceBus attribute below.
    // this will automagically pick up the 'value' of the app setting called 'ServiceBusConnectionString'
    [<Literal>]
    let ServiceBusConnectionString = "ServiceBusConnectionString"

    /// Convert the given event to a Service Bus message.
    let toMessage payload =
        Message(
            SessionId = payload.SessionId,
            Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)),
            ContentType = MessageContentType)

    [<FunctionName("FromHttpTriggerToServiceBusTopic")>]
    let run 
        ([<HttpTrigger(AuthorizationLevel.Function, "post", Route = null)>] req: HttpRequest)
        ([<ServiceBus(TopicName, Connection = ServiceBusConnectionString, EntityType = EntityType.Topic)>] topic: IAsyncCollector<Message>)
        (log: ILogger) = task {
            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask
            let myCustomData = JsonConvert.DeserializeObject<MyCustomType>(reqBody)

            // IAsyncCollector.AddAsync might potentially throw and in that case you're on your own (try / catch + retry?).
            do! topic.AddAsync(toMessage myCustomData)
            return OkObjectResult("Processed.") :> IActionResult }

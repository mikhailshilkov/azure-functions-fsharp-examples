namespace DurableFsharp

open Microsoft.Azure.WebJobs
open FSharp.Control.Tasks
open Microsoft.Azure.WebJobs.Host

module Counter =

  [<FunctionName("E3_Counter")>]
  let Run([<OrchestrationTrigger>] counterContext: DurableOrchestrationContext, log: TraceWriter) = task {
    let counterState = counterContext.GetInput<int>()
    log.Info(sprintf "Current counter state is %i. Waiting for next operation." counterState)

    let! command = counterContext.WaitForExternalEvent<string>("operation") 
    let operation = if isNull command then command else command.ToLowerInvariant()
    log.Info(sprintf "Received '%s' operation." operation)

    let newCounter = 
      match operation with
      | "incr" -> counterState + 1
      | "decr" -> counterState - 1
      | _ -> counterState

    if (operation <> "end") then counterContext.ContinueAsNew(newCounter)
    return newCounter
  }


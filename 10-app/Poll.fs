namespace PollApp

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.Azure.WebJobs.Extensions.Http

module Utils =
  let toOption (n: Nullable<_>) =
     if n.HasValue 
     then Some n.Value 
     else None

module Twitter =

  let run(tweet: string, pollResults: ICollector<string>, log: TraceWriter) =
    log.Info (sprintf "Tweet: %s" tweet)
    pollResults.Add tweet


module Landing =

  let run(req: HttpRequest, html: string, log: TraceWriter) =
    log.Info("F# HTTP trigger function processed a request." + html)

    ContentResult(Content = html, ContentType = "text/html")

[<CLIMutable>]
type PollResult = {
  value: string
}

type SentimentResult() = 
  inherit TableEntity()
  member val Text = "" with get, set
  member val Score = Nullable<double>() with get, set

module Send =

  let run(req: PollResult, pollResults: ICollector<string>, log: TraceWriter) =
    log.Info (sprintf "Poll result: %s" req.value)
    pollResults.Add req.value

module Sentiment =
  open Microsoft.Azure.CognitiveServices.Language.TextAnalytics
  open Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models

  let run (feedbackText, log: TraceWriter) =
    let client = new TextAnalyticsAPI()
    client.AzureRegion <- AzureRegions.Westeurope
    client.SubscriptionKey <- "65ff5b18c51c4a2ebd4505cbd5fe1e4b"

    let result = client.Sentiment(
                    MultiLanguageBatchInput(
                        [| MultiLanguageInput("en", "0", feedbackText) |]))

    let score = 
      result.Documents
      |> Seq.tryHead
      |> Option.map (fun x -> x.Score)
      |> Option.bind Utils.toOption

    let msg = 
      match score with
      | Some s -> sprintf "Sentiment of %s is %f" feedbackText s
      | None -> sprintf "No sentiment for %s" feedbackText
    log.Info msg

    let savedScore = 
      match score with 
      | Some s -> Nullable<float>(s)
      | None -> Nullable<float>()
      
    SentimentResult(PartitionKey = "default", RowKey = Guid.NewGuid().ToString(), Text = feedbackText, Score = savedScore)

module Results =
  open System.Linq  
  open Utils

  type Report = {
    AverageScore: double
    Messages: string list
  }

  let run(req: HttpRequest, resultsTable: CloudTable, log: TraceWriter) =
    
    let sentiments = resultsTable.ExecuteQuerySegmentedAsync(new TableQuery<SentimentResult>(), null).Result.AsEnumerable()
    let averageScore = 
      sentiments
      |> Seq.map (fun r -> r.Score)
      |> Seq.choose toOption
      |> Seq.average
    let messages = sentiments |> Seq.map (fun r -> r.Text) |> List.ofSeq

    log.Info(sprintf "Average score: %f" averageScore)

    ObjectResult({ AverageScore = averageScore; Messages = messages })


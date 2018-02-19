namespace HelloFunctions

open System
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host

module Say =
  let private daysUntil (d: DateTime) =
    (d - DateTime.Now).TotalDays |> int

  let hello (timer: TimerInfo, log: TraceWriter) =
    let christmas = new DateTime(2017, 12, 25)

    daysUntil christmas
    |> sprintf "%d days until Christmas"
    |> log.Info

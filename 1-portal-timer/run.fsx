open System

let minutesSince (d: DateTime) =
  (DateTime.Now - d).TotalMinutes

let run(myTimer: TimerInfo, log: TraceWriter) =
  let meetupStart = new DateTime(2017, 11, 8, 19, 0, 0)

  minutesSince meetupStart
  |> int
  |> sprintf "Our meetup has been running for %d minutes"
  |> log.Info
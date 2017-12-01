namespace HelloFunctions

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host

module Say =
  let hello (timer: TimerInfo, log: TraceWriter) =
    let now = System.DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
    log.Info (sprintf "Function ran at %s" now)

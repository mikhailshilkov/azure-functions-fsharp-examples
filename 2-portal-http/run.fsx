open System.Net

type Named = {
    name: string
}

let run (req: Named) (log: TraceWriter) = 
  log.Info (sprintf "Got a request from %s" req.name)
  
  let responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
  responseMessage.Content <- new StringContent("Hello " + req.name)
  responseMessage
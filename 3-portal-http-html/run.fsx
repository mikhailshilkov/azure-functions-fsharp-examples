open System.Text
open System.Net
open System.Net.Http

let Run(req: HttpRequestMessage, template: string, name: string) =
  let html = template.Replace("%name%", name)
  
  let response = req.CreateResponse(HttpStatusCode.OK)
  response.Content <- new StringContent(html, Encoding.UTF8, "text/html")
  response
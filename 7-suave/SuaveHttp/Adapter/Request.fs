module Suave.Azure.Functions.Request

open Suave.Http
open System.Net.Http
open System.Net.Http.Headers

let suaveHttpMethod (httpMethod : HttpMethod) =
   match httpMethod.Method with
   | "GET" -> HttpMethod.GET
   | "PUT" -> HttpMethod.PUT
   | "POST" -> HttpMethod.POST
   | "DELETE" -> HttpMethod.DELETE
   | "PATCH" -> HttpMethod.PATCH
   | "HEAD" -> HttpMethod.HEAD
   | "TRACE" -> HttpMethod.TRACE
   | "CONNECT" -> HttpMethod.CONNECT
   | "OPTIONS" -> HttpMethod.OPTIONS
   | x -> HttpMethod.OTHER x

let httpMethod (suaveHttpMethod : Suave.Http.HttpMethod) =  
  match suaveHttpMethod with
  | OTHER _ -> 
    None
  | _ -> 
    let method : System.String = suaveHttpMethod |> string
    let normalized = method.ToLowerInvariant()
    string(System.Char.ToUpper(normalized.[0])) + normalized.Substring(1)
    |> HttpMethod |> Some

let suaveHttpRequestHeaders (httpRequestHeaders : Headers.HttpRequestHeaders) = 
  httpRequestHeaders
  |> Seq.collect (fun h -> h.Value |> Seq.map (fun v -> (h.Key, v)))
  |> Seq.toList

let httpRequestHeaders (suaveHttpRequestHeaders : (string * string) list) (headers : HttpRequestHeaders) =
  suaveHttpRequestHeaders |> List.iter headers.Add
  headers

let suaveRawForm (content : HttpContent) = async {
  if isNull content then
   return Array.empty
  else 
    return! content.ReadAsByteArrayAsync() |> Async.AwaitTask
}

let httpContent (rawForm : byte[]) =
  new ByteArrayContent(rawForm) :> HttpContent

let suaveRawQuery (requestUri : System.Uri) =
  if requestUri.Query.Length > 1 then
    requestUri.Query.Substring(1)
  else
    ""

let suaveHttpRequest (httpRequestMessage : HttpRequestMessage) = async {
  let! rawForm = suaveRawForm httpRequestMessage.Content
  return {
    HttpRequest.empty with 
      headers = suaveHttpRequestHeaders httpRequestMessage.Headers
      url = httpRequestMessage.RequestUri
      rawForm = rawForm
      rawQuery = suaveRawQuery httpRequestMessage.RequestUri
      ``method`` = suaveHttpMethod httpRequestMessage.Method
      host = httpRequestMessage.RequestUri.Host
  }
}

let httpRequestMessage (suaveHttpRequest : HttpRequest) =   
  let req = new HttpRequestMessage()
  httpRequestHeaders suaveHttpRequest.headers req.Headers |> ignore
  req.RequestUri <- suaveHttpRequest.url
  req.Content <- new ByteArrayContent(suaveHttpRequest.rawForm)
  req.Method <- defaultArg (httpMethod suaveHttpRequest.method) (new HttpMethod("Unknown"))
  req
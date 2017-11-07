module Suave.Azure.Functions.Context

open Request
open Response
open Suave
open System.Net.Http
open System.Net

let runWebPart (app : WebPart) (httpRequestMessage : HttpRequestMessage) = async {
  let! req = suaveHttpRequest httpRequestMessage
  let! ctx = app { HttpContext.empty with request = req }
  match ctx with
  | Some ctx -> 
    return httpResponseMessage ctx.response
  | None -> 
    let res = new HttpResponseMessage()
    res.StatusCode <- HttpStatusCode.NotFound
    return res
}
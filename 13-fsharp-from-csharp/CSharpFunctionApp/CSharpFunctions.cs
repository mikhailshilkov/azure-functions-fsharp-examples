using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CSharpFunctionApp
{
    public static class CSharpFunctions
    {
        [FunctionName("CSharpLovesFSharp")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")]HttpRequest req, TraceWriter log)
        {
            var greeting = FSharpLibrary.Say.loves("C#", "F#");

            log.Info(greeting);

            return new OkObjectResult(greeting);
        }
    }
}

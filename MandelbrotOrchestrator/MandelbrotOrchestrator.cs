using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace MandelbrotOrchestrator
{
  public static class MandelbrotOrchestrator
  {

    [FunctionName("index")]
    public static IActionResult GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
    {
      var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
      return new ContentResult
      {
        Content = File.ReadAllText(path),
        ContentType = "text/html",
      };
    }

    [FunctionName("negotiate")]
    public static SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
        [SignalRConnectionInfo(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
    {
      return connectionInfo;
    }

    [FunctionName("MandelbrotOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      var outputs = new List<string>();

      // Replace "hello" with the name of your Durable Activity Function.
      outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
      DateTime dateTime = context.CurrentUtcDateTime.AddSeconds(15);
      await context.CreateTimer(dateTime, cancelToken: System.Threading.CancellationToken.None);
      outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
      outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

      // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
      return outputs;
    }

    [FunctionName(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] string name, ILogger log, [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
    {
      log.LogInformation($"Saying hello to {name}.");
      PublishMessage(signalRMessages, name);
      return $"Hello {name}!";
    }

    [FunctionName("MandelbrotOrchestrator_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
      // Function input comes from the request content.
      string instanceId = await starter.StartNewAsync("MandelbrotOrchestrator", null);

      log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

      return starter.CreateCheckStatusResponse(req, instanceId);
    }

    private static async void PublishMessage(IAsyncCollector<SignalRMessage> signalRMessages, string message)
    {
      await signalRMessages.AddAsync(
        new SignalRMessage
        {
          Target = "newMessage",
          Arguments = new[] { message }
        }
      );
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Mandelbrot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace MandelbrotOrchestrator
{
  public struct ComputeTask
  {
    public ComputeParameters ComputeParameters { get; }
    public string InstanceId { get; }

    public ComputeTask(ComputeParameters computeParameters, string instanceId)
    {
      ComputeParameters = computeParameters;
      InstanceId = instanceId;
    }
  }

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
    public static async Task RunOrchestrator(
      [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      // Replace "hello" with the name of your Durable Activity Function.
      //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
      //DateTime dateTime = context.CurrentUtcDateTime.AddSeconds(15);
      //await context.CreateTimer(dateTime, cancelToken: System.Threading.CancellationToken.None);
      int width = 1000;
      int height = 1000;
      ComputeParameters computeParameters = new(width: width, height: height, minX: -2.0, maxX: 1.0, minY: -1.5, maxY: 1.5, maxIterations: 10000);
      ComputeTask computeTask = new(computeParameters, context.InstanceId);
      await context.CallActivityAsync(nameof(ComputeMandelbrot), computeTask);
    }

    [FunctionName("ComputeMandelbrot")]
    public static async Task ComputeMandelbrot(
      [ActivityTrigger] ComputeTask computeTask,
      [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages
      )
    {
      MandelbrotBuilder mandelbrot = new();
      string pngBase64 = await mandelbrot.BuildPngAsBase64(computeTask.ComputeParameters);
      await signalRMessages.AddAsync(new SignalRMessage
      {
        Target = computeTask.InstanceId,
        Arguments = new[] { pngBase64, (object)computeTask.ComputeParameters },
      });
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

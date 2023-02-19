import * as signalR from "@microsoft/signalr";

interface OrchestratorResponse {
  id: string;
}

console.log("hello world");

const connect = (channelName: string) => {
  const apiBaseUrl = window.location.origin + "/api";
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(apiBaseUrl)
    .configureLogging(signalR.LogLevel.Information)
    .build();
  connection.on(channelName, (image, parameters) => {
    console.log(image);
    console.log(parameters);
  });

  connection.start().catch(console.error);
};

(() => {
  const canvas = document.getElementById("theCanvas") as HTMLCanvasElement;
  const ctx = canvas.getContext("2d");
  if (ctx) {
    ctx.fillStyle = "black";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
  }

  const startButton = document.getElementById(
    "startButton"
  ) as HTMLButtonElement;

  startButton.addEventListener("click", async () => {
    const response = await fetch("MandelbrotOrchestrator_HttpStart");
    var orchestratorResponse = (await response.json()) as OrchestratorResponse;

    connect(orchestratorResponse.id);
  });
})();

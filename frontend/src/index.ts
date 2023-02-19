import * as signalR from "@microsoft/signalr";

interface OrchestratorResponse {
  id: string;
}

interface ComputeParameters {
  MinX: number;
  MaxX: number;
  MinY: number;
  MaxY: number;
  Height: number;
  Width: number;
}

interface Frame {
  MinX: number;
  MaxX: number;
  MinY: number;
  MaxY: number;
}

console.log("hello world");

const myFrame: Frame = {
  MinX: -2.0,
  MaxX: 1.0,
  MinY: -1.5,
  MaxY: 1.5,
};

const mapMandelbrotSpaceToCanvas: (x: number, y: number) => [number, number] = (
  x: number,
  y: number
) => {
  const canvas = document.getElementById("theCanvas") as HTMLCanvasElement;
  const { width, height } = canvas;
  const mappedX = (width / (myFrame.MaxX - myFrame.MinX)) * (x - myFrame.MinX);
  const mappedY = (height / (myFrame.MinY - myFrame.MaxY)) * (y - myFrame.MaxY);
  return [mappedX, mappedY];
};

const connect = (channelName: string) => {
  const apiBaseUrl = window.location.origin + "/api";
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(apiBaseUrl)
    .configureLogging(signalR.LogLevel.Information)
    .build();
  connection.on(channelName, (image: string, parameters: ComputeParameters) => {
    console.log(image);
    console.log(parameters);
    const canvas = document.getElementById("theCanvas") as HTMLCanvasElement;
    const ctx = canvas.getContext("2d");
    if (ctx) {
      const [x, y] = mapMandelbrotSpaceToCanvas(
        parameters.MinX,
        parameters.MinY
      );
      console.log(`drawing on (${x}, ${y})`);
      const img = new Image();
      img.onload = () => {
        console.log("drawing");
        ctx.drawImage(img, x, y);
      };
      img.src = `data:image/png;base64,${image}`;
    }
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

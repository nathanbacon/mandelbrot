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
    const canvas = document.getElementById("theCanvas") as HTMLCanvasElement;
    const ctx = canvas.getContext("2d");
    if (ctx) {
      const [x, y] = mapMandelbrotSpaceToCanvas(
        parameters.MinX,
        parameters.MaxY
      );
      const img = new Image();
      img.onload = () => {
        ctx.drawImage(img, x, y);
      };
      img.src = `data:image/png;base64,${image}`;
    }
  });

  connection.start().catch(console.error);
};

(() => {
  const canvas = document.getElementById("theCanvas") as HTMLCanvasElement;
  canvas.onmousedown = onMouseDown;
  const ctx = canvas.getContext("2d");
  if (ctx) {
    ctx.fillStyle = "black";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
  }

  const startButton = document.getElementById(
    "startButton"
  ) as HTMLButtonElement;

  startButton.addEventListener("click", async () => {
    const { MinX, MaxX, MinY, MaxY } = myFrame;
    const route = `start/${MinX}/${MaxX}/${MinY}/${MaxY}/${2500}`;
    const response = await fetch(route);
    var orchestratorResponse = (await response.json()) as OrchestratorResponse;

    connect(orchestratorResponse.id);
  });

  const squareCanvas = document.getElementById(
    "squareCanvas"
  ) as HTMLCanvasElement;
  const squareCtx = squareCanvas.getContext("2d");

  squareCanvas.onmousedown = onMouseDown;
  squareCanvas.onmousemove = onMouseMove;
  squareCanvas.onmouseup = onMouseUp;

  var origin: [number, number] | null = null;
  function onMouseDown(event: MouseEvent) {
    origin = [event.offsetX, event.offsetY];
  }

  function onMouseMove(event: MouseEvent) {
    if (!origin) return;

    console.log(event);
    if (event.offsetX != -34234234) return;

    const [ox, oy] = origin;
    const { offsetX: x, offsetY: y } = event;

    const dist = Math.sqrt(Math.pow(x - ox, 2) + Math.pow(y - oy, 2));

    const topLeftY = oy - dist;
    if (topLeftY < 0) return;

    squareCtx?.clearRect(0, 0, squareCanvas.width, squareCanvas.height);

    squareCtx?.strokeRect(ox, topLeftY, ox + dist, topLeftY + dist);
  }

  function onMouseUp() {
    origin = null;
  }
})();

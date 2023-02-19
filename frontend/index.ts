import * as signalR from "@microsoft/signalr";

const apiBaseUrl = window.location.origin + "/api";
const connection = new signalR.HubConnectionBuilder()
  .withUrl(apiBaseUrl)
  .configureLogging(signalR.LogLevel.Information)
  .build();
connection.on("newMessage", (message) => {
  console.log("the message is: " + message);
});

console.log("hello, world");

connection.start().catch(console.error);
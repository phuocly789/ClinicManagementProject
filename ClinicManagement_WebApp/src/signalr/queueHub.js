import * as signalR from "@microsoft/signalr";

const token = localStorage.getItem("token");
const queueConnection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5066/queueHub",{
   
    accessTokenFactory: () => token,
}) // đổi khi deploy
  .withAutomaticReconnect()
  .build();
  export default queueConnection;
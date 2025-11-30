import * as signalR from "@microsoft/signalr";

const token = localStorage.getItem("token");
const queueConnection = new signalR.HubConnectionBuilder().withUrl("http://125.212.218.44:5066/queueHub",{
   
    accessTokenFactory: () => token,
}) // đổi khi deploy
  .withAutomaticReconnect()
  .build();
  export default queueConnection;
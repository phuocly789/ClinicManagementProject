import * as signalR from "@microsoft/signalr";

const token = localStorage.getItem("token");
const queueConnection = new signalR.HubConnectionBuilder().withUrl("http://clinicmanagementproject-1.onrender.com/queueHub",{
   
    accessTokenFactory: () => token,
}) // đổi khi deploy
  .withAutomaticReconnect()
  .build();
  export default queueConnection;
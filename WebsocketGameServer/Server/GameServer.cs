using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketGameServer.Controllers;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Data.Models.Rooms;
using WebsocketGameServer.Models.Args;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Server
{
    public class GameServer
    {
        private readonly Uri apiUrl = new Uri("");
        private IGameController gameController;


        public GameServer(IGameController controller)
        {
            this.gameController = controller;
        }

        public async void HandleNewSocketAsync(HttpContext context, WebSocket socket)
        {
            if (socket == null)
                return;

            var buffer = new ArraySegment<byte>(new byte[8192]);

            WebSocketReceiveResult res;
            using (var ms = new MemoryStream())
            {
                do
                {
                    res =
                        await socket
                            .ReceiveAsync(buffer, CancellationToken.None)
                            .ConfigureAwait(true);

                    ms.Write(buffer.Array, buffer.Offset, res.Count);
                } while (!res.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (res.MessageType == WebSocketMessageType.Text)
                {
                    string key = string.Empty;

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        key =
                            await reader
                                .ReadToEndAsync()
                                .ConfigureAwait(false);
                    }

                    if (string.IsNullOrEmpty(key))
                        return;

                    if (key[0] == '\"' && key[^1] == '\"')
                    {
                        key = key[1..^1];
                    }

                    PlayerVerificationResponseModel playerData =
                        await gameController
                            .VerifyAsync(key)
                            .ConfigureAwait(false);

                    if (playerData == null)
                        return;

                    await gameController
                        .AcceptPlayerAsync(new Player(playerData.Key, socket, playerData.PlayerId, playerData.Name))
                        .ConfigureAwait(false);

                    await HandleSocket(playerData.PlayerId, socket).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Sends updated room data to the api
        /// </summary>
        /// <param name="args">The room arguments of the room which state has been altered</param>
        public async void HandleNewRoomStateAsync(RoomArgs args)
        {
            if (args == null || args.Room == null)
                return;

            var request = WebRequest.CreateHttp(apiUrl);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Timeout = 10000;

            object payload = null;
            if (args.Room is ILobby lobby)
            {
                //TODO: add check for private lobby

                payload =
                    new GameRoomData(
                        lobby.GameType, 
                        lobby.RoomID, 
                        string.IsNullOrWhiteSpace(lobby.Name) ? string.Empty : lobby.Name,
                        lobby.MaxPlayersNeededToStart,
                        lobby.Players.Count);
            }
            //TODO: add other room subtypes

            if (payload == null)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            request.ContentLength = bytes.Length;

            using (Stream s = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                s.Write(bytes, 0, bytes.Length);
                s.Close();
            }
        }


        private async Task HandleSocket(long id, WebSocket socket)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(new byte[8192]);

            while (socket.State.Equals(WebSocketState.Open))
            {
                var msg = await socket.ReceiveAsync(segment, CancellationToken.None).ConfigureAwait(false);
                if (msg.MessageType.Equals(WebSocketMessageType.Text))
                {
                    string message = Encoding.UTF8.GetString(segment).Trim();
                    if (string.IsNullOrEmpty(message))
                        continue;

                    string[] args = message.Split('|');
                    if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
                        continue;

                    if (args[0].ToUpperInvariant().Equals("CREATE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //gamecontroller lobbyservice create call
                    }

                    IRoom room;
                    if (gameController.RoomManager.Rooms.TryGetValue(args[0], out room))
                    {
                        if (!string.IsNullOrEmpty(args[1]))
                        {
                            switch (args[1].ToUpperInvariant())
                            {
                                case "JOIN":
                                    if (gameController.Players.TryGetValue(new Player(id), out IPlayer playerData) &&
                                        await room.PlayerCanJoinRoom(playerData).ConfigureAwait(false))
                                        await gameController.RoomManager.AddPlayer(playerData, room.RoomID).ConfigureAwait(false);
                                    break;
                                case "LEAVE":
                                    await gameController.RoomManager.RemovePlayer(new Player(id), room.RoomID).ConfigureAwait(false);
                                    break;
                                default:
                                    gameController.RoomManager.Rooms[args[0]].ReceiveMessage(new RoomMessage(id, args[0], args[1..]));
                                    break;
                            }
                        }
                    }
                }
            }

            //socket closed, remove player from rooms and disconnect socket
            ICollection<IRoom> rooms;
            //create lookup player
            IPlayer player = new Player(id);
            //get rooms that the player is part of
            if (gameController.RoomManager.PlayerRooms.TryGetValue(player, out rooms) &&
                rooms != null)
            {
                //remove the player from all associated rooms
                foreach (string roomId in rooms.Select(x => x.RoomID))
                {
                    await gameController.RoomManager.RemovePlayer(player, roomId).ConfigureAwait(false);
                }
            }

            //disconnect player socket
            await DisconnectWebSocketAsync(socket).ConfigureAwait(false);
        }

        /// <summary>
        /// Disconnects a websocket asynchronously
        /// </summary>
        /// <param name="socket">The socket to be disconnected</param>
        /// <returns>The task object representing the disconnection of the socket</returns>
        private async Task DisconnectWebSocketAsync(WebSocket socket)
        {
            try
            {
                //close socket
                await socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None).ConfigureAwait(false);
                //dispose the socket
                socket.Dispose();
            }
            catch (Exception)
            {
                //TODO: error logging
            }
        }
    }
}

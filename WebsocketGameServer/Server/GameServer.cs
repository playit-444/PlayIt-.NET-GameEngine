using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketGameServer.Controllers;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Models.Args;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Server
{
    public class GameServer
    {
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

        //TODO:
        public async void HandleNewRoomStateAsync(RoomArgs args)
        {

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

            ICollection<IRoom> rooms;
            IPlayer player = new Player(id);
            if (gameController.RoomManager.PlayerRooms.TryGetValue(player, out rooms) &&
                rooms != null)
            {
                foreach (string roomId in rooms.Select(x => x.RoomID))
                {
                    await gameController.RoomManager.RemovePlayer(player, roomId).ConfigureAwait(false);
                }
            }
        }
    }
}

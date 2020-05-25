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
using WebsocketGameServer.Models.Args;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Server
{
    public class GameServer
    {
        private GameController gameController;

        public GameServer(GameController controller)
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
        public async void HandleNewRoomStateAsync(RoomArgs args) { 
        
        }


        private async Task HandleSocket(long id, WebSocket socket) { 
        
        }
    }
}

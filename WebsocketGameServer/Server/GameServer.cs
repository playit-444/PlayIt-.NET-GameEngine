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
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Server
{
    public class GameServer
    {
        private PlayerController playerController;

        public GameServer(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public async void HandleNewSocket(HttpContext context, WebSocket socket)
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

                    PlayerVerificationResponseModel playerData = 
                        await playerController
                            .VerifyAsync(key)
                            .ConfigureAwait(false);

                    if (playerData == null)
                        return;

                    await playerController
                        .AcceptPlayerAsync(new Player(playerData.Key, socket, playerData.PlayerId, playerData.Name))
                        .ConfigureAwait(false);
                }
            }
        }
    }
}

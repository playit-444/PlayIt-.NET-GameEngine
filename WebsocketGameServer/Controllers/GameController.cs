using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebsocketGameServer.Services.Security;
using WebsocketGameServer.Managers;
using WebsocketGameServer.Data.Game.Player;
using System.Net.WebSockets;
using GameServer.Services.Generators;
using WebsocketGameServer.Models.Player;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Services.Room;

namespace WebsocketGameServer.Controllers
{
    public class GameController : IGameController
    {
        private readonly IVerificationService<PlayerVerificationResponseModel> verificationService;

        public HashSet<IPlayer> Players { get; private set; }

        public IRoomManager RoomManager { get; }

        public GameController(IRoomManager roomManager, IVerificationService<PlayerVerificationResponseModel> VerificationService)
        {
            verificationService = VerificationService;
            RoomManager = roomManager;
        }

        public async Task<PlayerVerificationResponseModel> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            return await verificationService.VerifyToken(token).ConfigureAwait(false);
        }

        public async Task AcceptPlayerAsync(IPlayer player)
        {
            if (player == null)
                return;

            Players.Add(player);
        }
    }
}

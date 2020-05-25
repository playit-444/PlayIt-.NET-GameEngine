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
    public class GameController : IPlayerVerifier<PlayerVerificationResponseModel>
    {
        public HashSet<IPlayer> Players { get; private set; }

        public GameController(IRoomManager roomMan,
            IVerificationService<PlayerVerificationResponseModel> VerificationService, IIdentifierGenerator idGenerator,
            LobbyService lobbyService)
        {
            verificationService = VerificationService;
            roomManager = roomMan;
            _idGenerator = idGenerator;
            _lobbyService = lobbyService;
        }

        private readonly IVerificationService<PlayerVerificationResponseModel> verificationService;
        private readonly IRoomManager roomManager;
        private readonly IIdentifierGenerator _idGenerator;
        private readonly LobbyService _lobbyService;

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

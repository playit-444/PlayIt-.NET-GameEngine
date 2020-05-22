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

namespace WebsocketGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        public PlayerController(IPlayerManager PlayerManager, IVerificationService VerificationService) 
        {
            verificationService = VerificationService;
            playerManager = PlayerManager;
        }
        private readonly IVerificationService verificationService;
        private readonly IPlayerManager playerManager;
        private readonly ICollection<WebSocket> awaitingPlayers;

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            return await verificationService.VerifyToken(token).ConfigureAwait(false);                
        }

        public async Task AcceptplayerAsync(IPlayer player)
        {            
            await playerManager.AddPlayer(player).ConfigureAwait(false);
        }

        public void HandleNewSocket(HttpContext context, WebSocket socket)
        {
            
        }
    }
}
using System.Collections.Generic;
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
    public class PlayerController : ControllerBase, IPlayerController
    {
        public PlayerController(IPlayerManager playerManager, IVerificationService verificationService)
        {
            this.verificationService = verificationService;
            this.playerManager = playerManager;
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

        public async Task AcceptPlayerAsync(IPlayer player)
        {
            await playerManager.AddPlayer(player).ConfigureAwait(false);
        }

        public void HandleNewSocket(HttpContext context, WebSocket socket)
        {

        }
    }
}

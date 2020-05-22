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
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        public PlayerController(IPlayerManager PlayerManager, IVerificationService<PlayerVerificationResponseModel> VerificationService) 
        {
            verificationService = VerificationService;
            playerManager = PlayerManager;
        }
        private readonly IVerificationService<PlayerVerificationResponseModel> verificationService;
        private readonly IPlayerManager playerManager;

        public async Task<PlayerVerificationResponseModel> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            return await verificationService.VerifyToken(token).ConfigureAwait(false);
        }

        public async Task AcceptPlayerAsync(IPlayer player)
        {            
            await playerManager.AddPlayer(player).ConfigureAwait(false);
        }
    }
}
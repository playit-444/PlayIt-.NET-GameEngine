using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebsocketGameServer.Services.Security;
using WebsocketGameServer.Managers;
using WebsocketGameServer.Data.Game.Player;

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

        [HttpPost]
        [Route("verify/{token}")]
        public async Task<IActionResult> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest();

            if (await verificationService.VerifyToken(token).ConfigureAwait(false))
                return Ok();
            else
                return BadRequest();
        }

        [HttpPost]
        [Route("accept/{player}")]
        public async Task AcceptplayerAsync(IPlayer player)
        {
            
        }
    }
}
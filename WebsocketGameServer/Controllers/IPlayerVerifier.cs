﻿using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Controllers
{
    public interface IPlayerVerifier<T> where T : class
    {
        Task<T> VerifyAsync(string token);
        Task AcceptPlayerAsync(IPlayer player);
    }
}
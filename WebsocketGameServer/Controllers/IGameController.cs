using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Models.Player;

namespace WebsocketGameServer.Controllers
{
    public interface IGameController : IPlayerVerifier<PlayerVerificationResponseModel>
    {
        public IRoomManager RoomManager { get;}
        public HashSet<IPlayer> Players { get; }
    }
}

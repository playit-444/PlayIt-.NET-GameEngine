using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Services.Generators;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Models.Player;
using WebsocketGameServer.Services.Room;

namespace WebsocketGameServer.Controllers
{
    public interface IGameController : IPlayerVerifier<PlayerVerificationResponseModel>
    {
        public IRoomManager RoomManager { get; }
        public HashSet<IPlayer> Players { get; }
        public IIdentifierGenerator IdentifierGenerator { get; }
        public LobbyService LobbyService { get; }
    }
}

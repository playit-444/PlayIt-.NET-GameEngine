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
        /// <summary>
        /// The room manager of the game controller
        /// </summary>
        public IRoomManager RoomManager { get; }

        /// <summary>
        /// The list of players currently connected, indexed by their respective id
        /// </summary>
        public HashSet<IPlayer> Players { get; }

        /// <summary>
        /// The generator for making 'unique' ids for rooms mainly
        /// </summary>
        public IIdentifierGenerator IdentifierGenerator { get; }

        /// <summary>
        /// The lobby service responsible for creating lobby gamerooms
        /// </summary>
        public ILobbyService LobbyService { get; }
    }
}

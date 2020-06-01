using System.Collections.Generic;
using WebsocketGameServer.Services.Generators;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room.Lobbies;
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

        /// <summary>
        /// A list of the game type id and the name of the game attached to it
        /// </summary>
        public IDictionary<int, string> GameTypes { get; }

        /// <summary>
        /// The chatRoom for making new chatRooms for lobby and table
        /// </summary>
        public IChatRoomService ChatRoomService { get; }

        /// <summary>
        /// Event method for handing starting timer
        /// </summary>
        /// <param name="args"></param>
        public void HandleGameTimerStartEvent(GameStartEventArgs args);

        /// <summary>
        /// Event method for starting new games
        /// </summary>
        /// <param name="id"></param>
        public void HandleReadyTimerEnded(string id);
    }
}

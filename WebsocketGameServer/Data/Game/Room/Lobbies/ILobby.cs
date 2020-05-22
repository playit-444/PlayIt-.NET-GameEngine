using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Data.Game.Room.Lobbies
{
    public interface ILobby : IRoom
    {
        /// <summary>
        /// The type of game that will be played
        /// </summary>
        int GameType { get; }

        /// <summary>
        /// The name of the lobby instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The player state, stating whether they are ready to play
        /// </summary>
        IDictionary<IPlayer, bool> PlayerReadyState { get; }

        /// <summary>
        /// The minimum required players needed to start a game
        /// </summary>
        public byte MinPlayersNeededToStart { get; }


        /// <summary>
        /// The maximum required players needed to start a game
        /// </summary>
        public byte MaxPlayersNeededToStart { get; }
    }
}

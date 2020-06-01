using System.Collections.Generic;
using WebsocketGameServer.Data.Game.Players;

namespace WebsocketGameServer.Data.Game.Room.Lobbies
{
    public interface ILobby : IRoom
    {
        /// <summary>
        /// Event for starting timer
        /// </summary>
        public event GameTimeStartHandler OnTimerStart;

        public delegate void GameTimeStartHandler(GameStartEventArgs args);

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

    public class GameStartEventArgs
    {
        public string RoomId { get; private set; }
        public int MaximumPlayers { get; private set; }
        public int MinimumPlayers { get; private set; }
        public int ReadyPlayers { get; private set; }
        public IPlayer[] Players { get; private set; }

        public GameStartEventArgs(string roomId, int minimumPlayers, int maximumPlayers, int readyPlayers, IPlayer[] players)
        {
            RoomId = roomId;
            MaximumPlayers = maximumPlayers;
            MinimumPlayers = minimumPlayers;
            Players = players;
            ReadyPlayers = readyPlayers;
        }
    }
}

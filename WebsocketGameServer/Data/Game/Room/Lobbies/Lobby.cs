using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Data.Game.Room.Lobbies
{
    /// <summary>
    /// Standard lobby implementation for a simple game lobby
    /// </summary>
    public class Lobby : ILobby
    {
        private bool isDisposed = false; // To detect redundant dispose calls

        public Lobby(string roomID, string name, int gameType, IPlayer[] initialPlayers, byte minPlayersNeededToStart, byte maxPlayersNeededToStart)
        {
            RoomID = roomID;
            Name = name;
            GameType = gameType;
            MinPlayersNeededToStart = minPlayersNeededToStart;
            MaxPlayersNeededToStart = maxPlayersNeededToStart;


            if (initialPlayers == null)
                initialPlayers = Array.Empty<IPlayer>();
        }

        /// <summary>
        /// The minimum required players needed to start a game
        /// </summary>
        public byte MinPlayersNeededToStart { get; private set; }
        
        
        /// <summary>
        /// The maximum required players needed to start a game
        /// </summary>
        public byte MaxPlayersNeededToStart { get; private set; }
        
        public int GameType { get; private set; }
        public string Name { get; private set; }
        public IDictionary<IPlayer, bool> PlayerReadyState { get; private set; }
        public string RoomID { get; private set; }
        public HashSet<IPlayer> Players { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    PlayerReadyState.Clear();
                    Players.Clear();
                }

                isDisposed = true;
            }
        }

        ~Lobby()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

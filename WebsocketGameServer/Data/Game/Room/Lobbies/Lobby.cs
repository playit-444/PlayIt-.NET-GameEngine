using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Messages;

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

        public int GameType { get; private set; }
        public string Name { get; private set; }
        public IDictionary<IPlayer, bool> PlayerReadyState { get; private set; }
        public string RoomID { get; private set; }
        public HashSet<IPlayer> Players { get; private set; }

        public byte MinPlayersNeededToStart { get; private set; }

        public byte MaxPlayersNeededToStart { get; private set; }

        public virtual async Task<bool> PlayerCanJoinRoom(IPlayer player)
        {
            if (player == null || player.PlayerId.Equals(0) || player.Socket == null)
                return false;

            if (Players.Count >= (int)MaxPlayersNeededToStart)
                return false;

            return true;
        }

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

        public virtual void ReceiveMessage(IRoomMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Action))
                return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Data.Models.Players;
using WebsocketGameServer.Data.Models.Rooms;

namespace WebsocketGameServer.Data.Game.Room.Lobbies
{
    /// <summary>
    /// Standard lobby implementation for a simple game lobby
    /// </summary>
    public class Lobby : ILobby
    {
        private bool isDisposed = false; // To detect redundant dispose calls


        public Lobby(string roomID, string name, int gameType, IPlayer[] initialPlayers, byte minPlayersNeededToStart,
            byte maxPlayersNeededToStart)
        {
            RoomID = roomID;
            Name = name;
            GameType = gameType;
            MinPlayersNeededToStart = minPlayersNeededToStart;
            MaxPlayersNeededToStart = maxPlayersNeededToStart;
            Players = new HashSet<IPlayer>();
            PlayerReadyState = new Dictionary<IPlayer, bool>();

            if (initialPlayers == null)
                initialPlayers = Array.Empty<IPlayer>();
        }

        public event ILobby.GameTimeStartHandler OnTimerStart;
        public int GameType { get; private set; }
        public string Name { get; private set; }
        public string RoomID { get; private set; }

        public IDictionary<IPlayer, bool> PlayerReadyState { get; private set; }

        public HashSet<IPlayer> Players { get; private set; }

        public byte MinPlayersNeededToStart { get; private set; }

        public byte MaxPlayersNeededToStart { get; private set; }

        public virtual bool PlayerCanJoinRoom(IPlayer player)
        {
            if (player == null || player.PlayerId.Equals(0) || player.Socket == null)
                return false;

            if (Players.Count >= (int) MaxPlayersNeededToStart)
                return false;

            return true;
        }

        public virtual async void ReceiveMessage(IRoomMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Action))
                return;


            if (Players.TryGetValue(new Player(message.PlayerId), out IPlayer p))

                if (message.Action == "READY")
                {
                    if (PlayerReadyState.ContainsKey(p))
                    {
                        SetPlayerReadyState(p);
                    }

                    await SendMessageAsync().ConfigureAwait(false);
                }
        }

        private void SetPlayerReadyState(IPlayer player)
        {
            PlayerReadyState[player] = !PlayerReadyState[player];

            if (Players.Count >= MinPlayersNeededToStart)
            {
                int readyPlayers = 0;
                foreach (bool val in PlayerReadyState.Values)
                {
                    if (val)
                    {
                        readyPlayers++;
                    }
                }

                OnTimerStart?.Invoke(new GameStartEventArgs(RoomID, MinPlayersNeededToStart,
                    MaxPlayersNeededToStart, readyPlayers, Players.ToArray()));
            }
        }

        private async Task SendMessageAsync()
        {
            var playerDatas = new IPlayerData[Players.Count];
            for (int i = 0; i < Players.Count; i++)
            {
                var playerInformation = Players.ToArray()[i];
                bool readyState = false;
                if (PlayerReadyState.ContainsKey(playerInformation))
                {
                    readyState = PlayerReadyState[playerInformation];
                }

                playerDatas[i] = new PlayerData(playerInformation.PlayerId,
                    playerInformation.Name, readyState);
            }

            //Loop all players and tell a player left
            foreach (var roomPlayer in Players)
            {
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new LobbyData(playerDatas,
                            GameType,
                            RoomID, Name,
                            MaxPlayersNeededToStart,
                            playerDatas.Length, false)));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
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
    }
}

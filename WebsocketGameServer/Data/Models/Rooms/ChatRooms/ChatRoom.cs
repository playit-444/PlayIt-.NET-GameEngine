using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Messages;

namespace WebsocketGameServer.Data.Models.Rooms.ChatRooms
{
    public class ChatRoom : IChatRoom
    {
        public string Name { get; }
        public IDictionary<IPlayer, DateTime> LastMessages { get; }
        private bool isDisposed = false;
        public string RoomID { get; }
        public HashSet<IPlayer> Players { get; }

        public ChatRoom(string roomId, string name, IEnumerable<IPlayer> players)
        {
            Name = name;
            RoomID = roomId;
            Players = new HashSet<IPlayer>();

            if (players != null)
                foreach (var player in players)
                {
                    if (!Players.Contains(player))
                        Players.Add(player);
                }
        }

        public async void Message(IPlayer player, string message)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(message)) return;
            var args = new object[2];
            args[0] = message;
            args[1] = player.Name;
            string action;

            if (RoomID.Contains("LOBBYCHAT"))
            {
                action = "MSG|LOBBY";
            }
            else
            {
                action = "MSG|TABLE";
            }

            //Loop all players and tell a player left
            foreach (var roomPlayer in Players)
            {
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new RoomMessage(player.PlayerId, action, args)));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        public async void Whisper(IPlayer fromPlayer, IPlayer toPlayer, string message)
        {
            if (fromPlayer == null)
                throw new ArgumentNullException(nameof(fromPlayer));
            if (toPlayer == null)
                throw new ArgumentNullException(nameof(toPlayer));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            var args = new object[2];
            args[0] = message;
            var encoded =
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(new RoomMessage(fromPlayer.PlayerId, "MSG", args)));
            var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            args[1] = fromPlayer.Name;
            await fromPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                    CancellationToken.None)
                .ConfigureAwait(false);
            args[1] = toPlayer.Name;
            await toPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        public bool PlayerCanJoinRoom(IPlayer player)
        {
            if (Players.Contains(player))
                return false;
            return IChatRoom.MaxRoomSize > Players.Count;
        }

        public void ReceiveMessage(IRoomMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Action) || message.PlayerId == 0)
                throw new ArgumentNullException(nameof(message));
            if (message.Action.ToUpperInvariant() == "MSG")
            {
                Message(Players.Single(a => a.PlayerId == message.PlayerId), message.Args[0].ToString());
            }
            else if (message.Action.ToUpperInvariant() == "WHISPER")
            {
                if (long.TryParse(message.Args[0].ToString(), out long toPlayerId))
                    Whisper(Players.Single(a => a.PlayerId == message.PlayerId),
                        Players.Single(a => a.PlayerId == toPlayerId),
                        message.Args[0].ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Players.Clear();
                }

                isDisposed = true;
            }
        }

        ~ChatRoom()
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

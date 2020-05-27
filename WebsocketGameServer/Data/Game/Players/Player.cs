using System;
using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Players
{
    public class Player : IPlayer
    {
        public Player(string key, WebSocket socket, long playerId, string name)
        {
            Key = key;
            Socket = socket;
            PlayerId = playerId;
            Name = name;
        }

        public Player(string key)
        {
            Key = key;
        }

        public Player(long id)
        {
            PlayerId = id;
        }

        public string Key { get; set; }
        public WebSocket Socket { get; set; }
        public long PlayerId { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Player player &&
                   PlayerId == player.PlayerId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PlayerId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Player
{
    public class Player : IPlayer
    {
        public Player(string key, WebSocket socket, long playerID, string name)
        {
            Key = key;
            Socket = socket;
            PlayerID = playerID;
            Name = name;
        }

        public Player(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
        public WebSocket Socket { get; set; }
        public long PlayerID { get; set; }
        public string Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Player
{
    public interface IPlayer
    {
        string Key { get; set; }
        WebSocket Socket { get; set; }
        long PlayerID { get; set; }
        string Name { get; set; }
    }
}

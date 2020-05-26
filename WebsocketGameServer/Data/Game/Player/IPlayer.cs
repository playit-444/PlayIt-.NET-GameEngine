using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Player
{
    public interface IPlayer
    {
        string Key { get; set; }
        WebSocket Socket { get; set; }
        long PlayerId { get; set; }
        string Name { get; set; }
    }
}

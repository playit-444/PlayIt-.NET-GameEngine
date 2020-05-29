using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Players
{
    public interface IPlayerDataSimple
    {
        long PlayerId { get; set; }
        string Name { get; set; }
    }
}

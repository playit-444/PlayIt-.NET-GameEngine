using System.Net.WebSockets;

namespace WebsocketGameServer.Data.Game.Players
{
    public interface IPlayer
    {
        /// <summary>
        /// Unique key for player
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Socket for player is used for sending data via websocket
        /// </summary>
        WebSocket Socket { get; set; }

        /// <summary>
        /// PlayerId of player
        /// </summary>
        long PlayerId { get; set; }

        /// <summary>
        /// Name of player
        /// </summary>
        string Name { get; set; }
    }
}

using System.Collections.Generic;
using WebsocketGameServer.Data.Game.Players;

namespace WebsocketGameServer.Data.Models.Rooms.ChatRooms
{
    public class RestrictedChatRoom : ChatRoom
    {
        private float messageTimeout { get; }

        public RestrictedChatRoom(float a, string roomId, string name, IEnumerable<IPlayer> players) : base(roomId,
            name, players)
        {
        }
    }
}

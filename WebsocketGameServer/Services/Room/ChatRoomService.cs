using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Models.Rooms.ChatRooms;

namespace WebsocketGameServer.Services.Room
{
    public class ChatRoomService : IChatRoomService
    {
        public IChatRoom CreateChatRoom(string roomId, IPlayer[] players, string name = null)
        {
            return new ChatRoom(roomId, name, players);
        }

        public IChatRoom CreateRestrictedChatRoom(string roomId, IPlayer[] players, float timeOut, string name = null)
        {
            return new RestrictedChatRoom(timeOut, roomId, name, players);
        }
    }
}

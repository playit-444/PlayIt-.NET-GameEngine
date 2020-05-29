using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Models.Rooms.ChatRooms;

namespace WebsocketGameServer.Services.Room
{
    /// <summary>
    /// Interface defining the creation of a chat room service which is capable of creating different sorts of chat rooms
    /// </summary>
    public interface IChatRoomService
    {
        IChatRoom CreateChatRoom(string roomId, IPlayer[] players, string name = null);
        IChatRoom CreateRestrictedChatRoom(string roomId, IPlayer[] players, float timeOut, string name = null);
    }
}

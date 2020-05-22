using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Services.Room
{
    public interface IRoomManager
    {
        public IDictionary<string, IRoom> Rooms { get; }
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; }
        public Task<bool> AddPlayer (IPlayer player, string roomId);
        public Task<bool> RemovePlayer(IPlayer player, string roomId);
        public void AddRoom(IRoom room);
        public void RemoveRoom(string roomId);
    }
}

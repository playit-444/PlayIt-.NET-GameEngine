using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Services.Room
{
    public class RoomManager : IRoomManager
    {
        public IDictionary<string, IRoom> Rooms()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> AddPlayer(IPlayer player, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemovePlayer(IPlayer player)
        {
            throw new System.NotImplementedException();
        }

        public void AddRoom(IRoom room)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRoom(string room)
        {
            throw new System.NotImplementedException();
        }
    }
}

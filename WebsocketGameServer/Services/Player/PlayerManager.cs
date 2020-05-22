using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Services.Player
{
    public class PlayerManager : IPlayerManager
    {
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> AddPlayer(IPlayer player, IRoom[] rooms = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemovePlayer(IPlayer player)
        {
            throw new System.NotImplementedException();
        }
    }
}

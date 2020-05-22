using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;

namespace WebsocketGameServer.Services.Player
{
    public interface IPlayerManager
    {
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms();
        public Task<bool> AddPlayer(IPlayer player, IRoom[] rooms = null);
        public Task<bool> RemovePlayer(IPlayer player);
    }
}

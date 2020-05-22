using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Services.Generators;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Services.Generators;

namespace WebsocketGameServer.Services.Room
{
    public class RoomManager : IRoomManager
    {
        public IDictionary<string, IRoom> Rooms { get; }
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; }

        public Task<bool> AddPlayer(IPlayer player, string roomId)
        {
            //Rooms[roomId] as ILob

            //Check if player exists in playerRooms else add
            if (!PlayerRooms.ContainsKey(player))
            {
                PlayerRooms.Add(player, new List<IRoom>());
            }

            PlayerRooms[player].Add(Rooms[roomId]);

            Rooms[roomId].Players.Add(player);
            return new Task<bool>(() => true);
        }

        public Task<bool> RemovePlayer(IPlayer player, string roomId)
        {
            if (Rooms[roomId].Players.Contains(player))
            {
                PlayerRooms.Remove(player);
                Rooms[roomId].Players.Remove(player);
                return new Task<bool>(() => true);
            }

            return new Task<bool>(() => false);
        }

        public void AddRoom(IRoom room)
        {
            IIdentifierGenerator idGenerator = new HexIdGenerator();

            //Create unique id for room
            string id;
            do
            {
                id = idGenerator.CreateID(6);
            } while (Rooms.ContainsKey(id));

            Rooms.Add(id, room);
        }

        public void RemoveRoom(string roomId)
        {
            Rooms.Remove(roomId);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Services.Generators;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Services.Generators;

namespace WebsocketGameServer.Managers.Room
{
    public class RoomManager : IRoomManager
    {
        /// <summary>
        /// List of rooms with id for fast lookup
        /// </summary>
        public IDictionary<string, IRoom> Rooms { get; private set; }

        /// <summary>
        /// List of rooms player is in for fast lookup
        /// </summary>
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; private set; }

        /// <summary>
        /// Add a player to a specific lobby
        /// Also getting added to playerrooms for fast lookup
        /// </summary>
        /// <param name="player"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<bool> AddPlayer(IPlayer player, string roomId)
        {
            if (player == null || string.IsNullOrEmpty(roomId))
                return false;

            if (Rooms.ContainsKey(roomId) && await Rooms[roomId].PlayerCanJoinRoom(player).ConfigureAwait(false))
            {
                Rooms[roomId].Players.Add(player);

                if (PlayerRooms.ContainsKey(player))
                    PlayerRooms[player].Add(Rooms[roomId]);
                else
                    PlayerRooms.Add(player, new List<IRoom>() { Rooms[roomId] });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove player from rooms and playerRooms
        /// </summary>
        /// <param name="player"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<bool> RemovePlayer(IPlayer player, string roomId)
        {
            if (player == null || string.IsNullOrEmpty(roomId))
                return false;

            if (Rooms[roomId].Players.Contains(player))
            {
                PlayerRooms[player].Remove(Rooms[roomId]);
                Rooms[roomId].Players.Remove(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add new room to rooms
        /// </summary>
        /// <param name="room"></param>
        public void AddRoom(IRoom room)
        {
            if (room == null || Rooms.ContainsKey(room.RoomID))
                return;

            Rooms.Add(room.RoomID, room);
        }

        /// <summary>
        /// Remove room
        /// </summary>
        /// <param name="roomId"></param>
        public void RemoveRoom(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
                return;

            Rooms.Remove(roomId);
        }
    }
}

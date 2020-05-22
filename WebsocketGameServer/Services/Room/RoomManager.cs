using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Services.Generators;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Services.Generators;

namespace WebsocketGameServer.Services.Room
{
    public class RoomManager : IRoomManager
    {
        /// <summary>
        /// List of rooms with id for fast lookup
        /// </summary>
        public IDictionary<string, IRoom> Rooms { get; }

        /// <summary>
        /// List of rooms player is in for fast lookup
        /// </summary>
        public IDictionary<IPlayer, ICollection<IRoom>> PlayerRooms { get; }

        /// <summary>
        /// Add a player to a specific lobby
        /// Also getting added to playerrooms for fast lookup
        /// </summary>
        /// <param name="player"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public Task<bool> AddPlayer(IPlayer player, string roomId)
        {
            int maxPlayers = 0;
            if (Rooms[roomId] is Lobby lobby)
                maxPlayers = lobby.MaxPlayersNeededToStart;

            //Check if room is filed before processing
            if (maxPlayers < Rooms[roomId].Players.Count)
            {
                //Check if player exists in playerRooms else add
                if (!PlayerRooms.ContainsKey(player))
                {
                    PlayerRooms.Add(player, new List<IRoom>());
                }

                //Add room to player room
                PlayerRooms[player].Add(Rooms[roomId]);
                //Add player to room
                Rooms[roomId].Players.Add(player);
                return new Task<bool>(() => true);
            }

            return new Task<bool>(() => false);
        }

        /// <summary>
        /// Remove player from rooms and playerRooms
        /// </summary>
        /// <param name="player"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add new room to rooms
        /// </summary>
        /// <param name="room"></param>
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

        /// <summary>
        /// Remove room
        /// </summary>
        /// <param name="roomId"></param>
        public void RemoveRoom(string roomId)
        {
            Rooms.Remove(roomId);
        }
    }
}

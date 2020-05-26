using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Services.Generators;
using WebsocketGameServer.Data.Constants;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Models.Args;
using WebsocketGameServer.Services.Generators;
using WebsocketGameServer.Services.Security;

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

        public event IRoomManager.RoomHandler RoomStateChanged;

        public RoomManager()
        {
            Rooms = new Dictionary<string, IRoom>();
        }

        /// <summary>
        /// Add a player to a specific lobby
        /// Also getting added to playerrooms for fast lookup
        /// </summary>
        /// <param name="player">The player to add to the room</param>
        /// <param name="roomId">The id of the room to add the player to</param>
        /// <returns>Whether the player was successfully added to the room</returns>
        public async Task<bool> AddPlayer(IPlayer player, string roomId)
        {
            //check nulls
            if (player == null || player.PlayerId.Equals(0) || string.IsNullOrEmpty(player.Name))
                throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(roomId))
                throw new ArgumentNullException(nameof(roomId));

            //make sure the room exists and that the player can join
            if (Rooms.ContainsKey(roomId) && Rooms[roomId].PlayerCanJoinRoom(player))
            {
                //add the player to the room
                Rooms[roomId].Players.Add(player);

                //add the room to the player rooms map
                if (PlayerRooms.ContainsKey(player))
                    PlayerRooms[player].Add(Rooms[roomId]);
                else
                    PlayerRooms.Add(player, new List<IRoom>() {Rooms[roomId]});

                //invoke event to notify listerners of the new state of the room
                RoomStateChanged?.Invoke(new RoomArgs(Rooms[roomId], RoomActionType.UPDATE));

                //return with a successfull player insert
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove player from rooms and playerRooms
        /// </summary>
        /// <param name="player">The player to be removed from the room</param>
        /// <param name="roomId">The id of the room which the player should be removed from</param>
        /// <returns>Whether the player was successfully removed from the room</returns>
        public async Task<bool> RemovePlayer(IPlayer player, string roomId)
        {
            //check null values
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(roomId))
                throw new ArgumentNullException(nameof(roomId));

            //make sure the player is in the room
            if (Rooms[roomId].Players.Contains(player))
            {
                //remove both bindings from the player to the room
                PlayerRooms[player].Remove(Rooms[roomId]);
                Rooms[roomId].Players.Remove(player);

                //check whether the room is now empty and delete if that's the case
                if (Rooms[roomId].Players.Count < 1)
                {
                    //invoke event to notify listeners of the deleted room
                    RoomStateChanged?.Invoke(new RoomArgs(Rooms[roomId], RoomActionType.DELETE));

                    //remove the room
                    RemoveRoom(roomId);
                    return true;
                }

                //invoke event to notify listeners of the updated room
                RoomStateChanged?.Invoke(new RoomArgs(Rooms[roomId], RoomActionType.UPDATE));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Add new room to rooms
        /// </summary>
        /// <param name="room">The room to be added</param>
        public void AddRoom(IRoom room)
        {
            //check nulls
            if (room == null || string.IsNullOrEmpty(room.RoomID))
                throw new ArgumentNullException(nameof(room));

            //check rooms for duplicate value
            if (Rooms.ContainsKey(room.RoomID))
                return;

            //add room
            Rooms.Add(room.RoomID, room);

            //notify listeners
            RoomStateChanged?.Invoke(new RoomArgs(room, RoomActionType.CREATE));
        }

        /// <summary>
        /// Remove room
        /// </summary>
        /// <param name="roomId">The id of the room to be removed</param>
        public void RemoveRoom(string roomId)
        {
            //check nulls
            if (string.IsNullOrEmpty(roomId))
                throw new ArgumentNullException(nameof(roomId));

            //check whether the room exists
            if (!Rooms.ContainsKey(roomId))
                return;

            //notify listeners of the deleted room
            RoomStateChanged?.Invoke(new RoomArgs(Rooms[roomId], RoomActionType.DELETE));

            //remove room
            Rooms.Remove(roomId);
        }
    }
}

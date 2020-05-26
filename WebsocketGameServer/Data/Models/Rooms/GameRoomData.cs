using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Data.Models.Rooms
{
    public class GameRoomData : IGameRoomData
    {
        public GameRoomData(in int gameType, in string roomID, in string name, in int maxUsers, in int currentUsers, in bool privateRoom = false)
        {
            GameType = gameType;
            RoomID = roomID;
            Name = name;
            MaxUsers = maxUsers;
            CurrentUsers = currentUsers;
            PrivateRoom = privateRoom;
        }

        public int GameType { get; private set; }

        public string RoomID { get; private set; }

        public string Name { get; private set; }

        public int MaxUsers { get; private set; }

        public int CurrentUsers { get; private set; }

        public bool PrivateRoom { get; private set; }
    }
}

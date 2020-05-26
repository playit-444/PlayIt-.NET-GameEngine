using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Players;

namespace WebsocketGameServer.Data.Models.Rooms
{
    public class LobbyData : ILobbyData
    {
        public LobbyData(IPlayerData[] players, int gameType, string roomID, string name, int maxUsers, int currentUsers, bool privateRoom = false)
        {
            Players = players;
            GameType = gameType;
            RoomID = roomID;
            Name = name;
            MaxUsers = maxUsers;
            CurrentUsers = currentUsers;
            PrivateRoom = privateRoom;
        }

        public IPlayerData[] Players { get; private set; }

        public int GameType { get; private set; }

        public string RoomID { get; private set; }

        public string Name { get; private set; }

        public int MaxUsers { get; private set; }

        public int CurrentUsers { get; private set; }

        public bool PrivateRoom { get; private set; }
    }
}

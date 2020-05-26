using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Players;

namespace WebsocketGameServer.Data.Models.Rooms
{
    public interface ILobbyData : IGameRoomData
    {
        public IPlayerData[] Players { get;}
    }
}

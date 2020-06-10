using WebsocketGameServer.Data.Models.Players;

namespace WebsocketGameServer.Data.Models.Rooms
{
    public interface ILobbyData : IGameRoomData
    {
        public IPlayerData[] Players { get; }
    }
}

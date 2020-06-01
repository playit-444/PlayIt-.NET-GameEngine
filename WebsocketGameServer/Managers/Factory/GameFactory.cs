using WebsocketGameServer.Data.Game.Playable;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;

namespace WebsocketGameServer.Managers.Factory
{
    public class GameFactory
    {
        public IRoom CreateGame(IRoom room)
        {
            IRoom game = null;
            if (room is ILobby la)
            {
                switch (la.GameType)
                {
                    case 1:
                        game = new LudoStandard(la.GameType, 60, la.RoomID, la.Players);
                        break;
                    case 2:
                        game = new LudoStandard(la.GameType, 60, la.RoomID, la.Players);
                        break;
                    default:
                        game = new LudoStandard(la.GameType, 60, la.RoomID, la.Players);
                        break;
                }
            }

            return game;
        }
    }
}

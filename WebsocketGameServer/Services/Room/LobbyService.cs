using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room.Lobbies;

namespace WebsocketGameServer.Services.Room
{
    public class LobbyService : ILobbyService
    {
        public ILobby CreateLobby(in string id, in int gameType, IPlayer[] players, in byte minPlayers,
            in byte maxPlayers,
            in string name = null)
        {
            return new Lobby(id, name ?? string.Empty, gameType, players, minPlayers, maxPlayers);
        }

        public ILobby CreatePrivateLobby(in int gameType, IPlayer[] players, in byte minPlayers, in byte maxPlayers,
            in string name = null, in string password = null)
        {
            throw new System.NotImplementedException();
        }

        public ILobby CreateRankedLobby(in int gameType, IPlayer[] players)
        {
            throw new System.NotImplementedException();
        }
    }
}

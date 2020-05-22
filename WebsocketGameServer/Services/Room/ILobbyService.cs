using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Game.Room.Lobbies;

namespace WebsocketGameServer.Services.Room
{
    public interface ILobbyService
    {

        /// <summary>
        /// Creates a standard publicly joinable lobby
        /// </summary>
        /// <param name="gameType">The time of game that will be played in the room</param>
        /// <param name="players">The players the room will start off with</param>
        /// <param name="minPlayers">The minimum players needed to start the game</param>
        /// <param name="maxPlayers">The maximum amount of players allowed in the game</param>
        /// <param name="name">The name of the lobby</param>
        /// <returns>A new instance of ILobby</returns>
        ILobby CreateLobby(in int gameType, IPlayer[] players, in byte minPlayers, in byte maxPlayers, in string name = null);

        /// <summary>
        /// Creates a private lobby room
        /// </summary>
        /// <param name="gameType">The time of game that will be played in the room</param>
        /// <param name="players">The players the room will start off with</param>
        /// <param name="minPlayers">The minimum players needed to start the game</param>
        /// <param name="maxPlayers">The maximum amount of players allowed in the game</param>
        /// <param name="name">The name of the lobby</param>
        /// <param name="password"></param>
        /// <returns>A new instance of ILobby with a password</returns>
        ILobby CreatePrivateLobby(in int gameType, IPlayer[] players, in byte minPlayers, in byte maxPlayers, in string name = null, in string password = null);

        /// <summary>
        /// Creates a ranked mode game lobby
        /// </summary>
        /// <param name="gameType">The time of game that will be played in the room</param>
        /// <param name="players">The players the room will start off with</param>
        /// <returns>A new instance of ILobby with specific rules based on the competetive version of the gametype ruleset</returns>
        ILobby CreateRankedLobby(in int gameType, IPlayer[] players);
    }
}

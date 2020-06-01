using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Players;

namespace WebsocketGameServer.Controllers
{
    public interface IPlayerVerifier<T> where T : class
    {
        /// <summary>
        /// Verify if player is logged in
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T> VerifyAsync(string token);

        /// <summary>
        /// Add new player to list of players
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Task AcceptPlayerAsync(IPlayer player);
    }
}

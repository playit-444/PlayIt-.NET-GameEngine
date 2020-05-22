using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Controllers
{
    public interface IPlayerController
    {
        Task<bool> VerifyAsync(string jwtToken);
        Task AcceptPlayerAsync(IPlayer player);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using WebsocketGameServer.Services.Security;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Models.Player;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Services.Generators;
using WebsocketGameServer.Services.Room;

namespace WebsocketGameServer.Controllers
{
    public class GameController : IGameController
    {
        private readonly IVerificationService<PlayerVerificationResponseModel> verificationService;

        public HashSet<IPlayer> Players { get; private set; }

        public IIdentifierGenerator IdentifierGenerator { get; private set; }
        public ILobbyService LobbyService { get; }
        public IChatRoomService ChatRoomService { get; }
        public IRoomManager RoomManager { get; }

        public IDictionary<int, string> GameTypes { get; private set; }

        public GameController(
            IRoomManager roomManager,
            IVerificationService<PlayerVerificationResponseModel> VerificationService,
            IIdentifierGenerator identifierGenerator,
            ILobbyService lobbyService,
            IChatRoomService chatRoomService)
        {
            verificationService = VerificationService;
            RoomManager = roomManager;
            IdentifierGenerator = identifierGenerator;
            LobbyService = lobbyService;
            ChatRoomService = chatRoomService;

            Players = new HashSet<IPlayer>();
            GameTypes = new Dictionary<int, string>();
        }

        public async Task<PlayerVerificationResponseModel> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            return await verificationService.VerifyToken(token).ConfigureAwait(false);
        }

        public async Task AcceptPlayerAsync(IPlayer player)
        {
            if (player == null)
                return;

            Players.Add(player);
        }
    }
}

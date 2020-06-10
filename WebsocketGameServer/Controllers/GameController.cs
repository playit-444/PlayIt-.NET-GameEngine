using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebsocketGameServer.Services.Security;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Managers.Factory;
using WebsocketGameServer.Models.Player;
using WebsocketGameServer.Managers.Room;
using WebsocketGameServer.Managers.Timers;
using WebsocketGameServer.Services.Generators;
using WebsocketGameServer.Services.Room;

namespace WebsocketGameServer.Controllers
{
    public class GameController : IGameController, IPlayerVerifier<PlayerVerificationResponseModel>
    {
        private readonly IVerificationService<PlayerVerificationResponseModel> verificationService;

        public HashSet<IPlayer> Players { get; private set; }

        public IIdentifierGenerator IdentifierGenerator { get; private set; }
        public ILobbyService LobbyService { get; }
        public IChatRoomService ChatRoomService { get; }
        public IRoomManager RoomManager { get; }
        public IDictionary<int, string> GameTypes { get; private set; }

        public ReadyTimer ReadyTimer;
        public GameFactory GameFactory;

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

            GameFactory = new GameFactory();
            ReadyTimer = new ReadyTimer();
            Thread timerThread = new Thread(ReadyTimer.StartTimer);
            timerThread.Start();

            Players = new HashSet<IPlayer>();
            GameTypes = new Dictionary<int, string>();
        }

        public void HandleGameTimerStartEvent(GameStartEventArgs args)
        {
            //TODO SET TO 60
            ReadyTimer.AddTimer(args, 2);
            ReadyTimer.OnTimerEnd += HandleReadyTimerEnded;
        }

        /// <summary>
        /// Handles room creation once the ready timer has ended
        /// </summary>
        /// <param name="id">the id of the room</param>
        public void HandleReadyTimerEnded(string id)
        {
            //make sure room exists
            if (RoomManager.Rooms.ContainsKey(id))
            {
                //create a game room to replace the existing room
                var game = GameFactory.CreateGame(RoomManager.Rooms[id]);
                if (game != null)
                {
                    //replace the room
                    RoomManager.RemoveRoom(game.RoomID);
                    RoomManager.AddRoom(game);
                }
            }
        }

        public async Task<PlayerVerificationResponseModel> VerifyAsync(string token)
        {
            //check null
            if (string.IsNullOrEmpty(token))
                return null;

            //return the verification result
            return await verificationService.VerifyToken(token).ConfigureAwait(false);
        }

        public async Task AcceptPlayerAsync(IPlayer player)
        {
            //check null
            if (player == null)
                return;

            //add player to the list
            Players.Add(player);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketGameServer.Controllers;
using WebsocketGameServer.Data.Constants;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Game.Room.Lobbies;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Data.Models.Rooms;
using WebsocketGameServer.Data.Models.Types;
using WebsocketGameServer.Data.Models.Players;
using WebsocketGameServer.Models.Args;
using WebsocketGameServer.Models.Player;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Models.Rooms.ChatRooms;

namespace WebsocketGameServer.Server
{
    public class GameServer
    {
        private readonly string baseUrl = "https://api.444.dk/api/";

        //private readonly string baseUrl = "https://localhost:5002/api/";
        public PlayerJwtTokenModel playerJwtTokenModel;
        private IGameController gameController;

        public GameServer(IGameController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            gameController = controller;
            gameController.RoomManager.RoomStateChanged += HandleNewRoomStateAsync;
        }

        ~GameServer()
        {
            WebRequest request = WebRequest.Create(new Uri(baseUrl + "Game/ServerClose"));
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + playerJwtTokenModel.JwtToken);
            request.GetResponse();
        }

        private void AddGameType(GameTypeData data)
        {
            if (!gameController.GameTypes.ContainsKey(data.gameTypeId))
            {
                gameController.GameTypes.Add(data.gameTypeId, data.name);

                for (int i = 0; i < 3; i++)
                {
                    var roomId = gameController.IdentifierGenerator.CreateID(4);

                    var lobby = gameController.LobbyService
                        .CreateLobby(
                            roomId,
                            data.gameTypeId,
                            Array.Empty<IPlayer>(),
                            (byte) data.minimumPlayers,
                            (byte) data.maxPlayers,
                            $"thunberg deluxe {data.name}");

                    gameController.RoomManager
                        .AddRoom(lobby);

                    //Add chatRoom to auto generated rooms
                    gameController.RoomManager.AddRoom(
                        gameController.ChatRoomService.CreateChatRoom(roomId + "-TABLECHAT",
                            null, null));
                }

                gameController.RoomManager.AddRoom(
                    gameController.ChatRoomService.CreateChatRoom(
                        data.gameTypeId.ToString(CultureInfo.CurrentCulture) + "-LOBBYCHAT",
                        null, null));
            }
        }

        public async void Initialize()
        {
            //Login and save JWTToken for later use
            var request = WebRequest.CreateHttp(new Uri(baseUrl + "Account/signin"));
            request.ContentType = "application/json";
            request.Timeout = 10000;
            request.Method = "POST";

            //get bytes of the json representation of the payload object and set the payload size
            byte[] bytes =
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(new PlayerSignIn("api.444", "tc5mAM!NIRDp%dr5", "127.0.0.1")));
            request.ContentLength = bytes.Length;

            //write&flush content to the uri endpoint
            using (Stream s = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await s.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                await s.FlushAsync().ConfigureAwait(false);

                //close stream
                s.Close();
            }

            //Wait for response for the post request
            var response = (HttpWebResponse) request.GetResponse();
            var streamResponse = new StreamReader(response.GetResponseStream());
            //Get token from api call
            playerJwtTokenModel =
                JsonConvert.DeserializeObject<PlayerJwtTokenModel>(await streamResponse.ReadToEndAsync());

            streamResponse.Dispose();

            //get game types
            //create a new http request
            request = WebRequest.CreateHttp(new Uri(baseUrl + "GameType/simple"));
            request.ContentType = "application/json";
            request.Timeout = 10000;
            request.Method = "GET";

            var gametypeRes = request.GetResponse();
            StreamReader reader = new StreamReader(gametypeRes.GetResponseStream());
            IList<GameTypeData> jsonRes = JsonConvert.DeserializeObject<List<GameTypeData>>(reader.ReadToEnd());
            reader.Dispose();

            foreach (GameTypeData gameType in jsonRes)
            {
                AddGameType(gameType);
            }
        }

        /// <summary>
        /// Handles a new socket connection, and begins the verification process before talking back and forth with the socket
        /// </summary>
        /// <param name="context">The http context that the socket connected through</param>
        /// <param name="socket">The socket attempting to get accepted</param>
        public async Task HandleNewSocketAsync(HttpContext context, WebSocket socket)
        {
            byte[] buf = new byte[4096];
            //check nulls
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            //declare temptorary buffer
            ArraySegment<byte> buffer = new ArraySegment<byte>(buf);

            //temporary receive result
            WebSocketReceiveResult res =
                await socket
                    .ReceiveAsync(buffer, CancellationToken.None)
                    .ConfigureAwait(true);

            long playerId = 0;

            //check the message type
            if (!res.CloseStatus.HasValue)
            {
                string key = string.Empty;

                key = Encoding.UTF8.GetString(new ArraySegment<byte>(buf, 0, res.Count).Array);

                //check nulls
                if (string.IsNullOrEmpty(key))
                    return;

                //TODO under dosent work so this is a quick fix. think it something about empty chars at end
                key = key.Split("\"")[1];

                //verify the user
                PlayerVerificationResponseModel playerData =
                    await gameController
                        .VerifyAsync(key).ConfigureAwait(false);

                //check nulls
                if (playerData == null || playerData.PlayerId.Equals(0))
                {
                    // Verification failed
                    var encoded = Encoding.UTF8.GetBytes("{\"Authentication\":\"Error\"}");
                    var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                    await socket.SendAsync(buffers, WebSocketMessageType.Text, true, CancellationToken.None)
                        .ConfigureAwait(false);
                    return;
                }
                else
                {
                    // Verification success
                    var encoded = Encoding.UTF8.GetBytes("{\"Authentication\":\"Success\"}");
                    var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                    await socket.SendAsync(buffers, WebSocketMessageType.Text, true, CancellationToken.None)
                        .ConfigureAwait(false);
                }

                playerId = playerData.PlayerId;

                //accept the player socket and add it to the gamecontroller list of players
                await gameController
                    .AcceptPlayerAsync(new Player(playerData.Key, socket, playerData.PlayerId, playerData.Name))
                    .ConfigureAwait(false);
            }

            //keep receiving data while the socket is open
            while (!res.CloseStatus.HasValue)
            {
                //accept text only as of now //TODO: swap to binary?
                if (res.MessageType.Equals(WebSocketMessageType.Text))
                {
                    //get the string content and skip if that content turns out to be null
                    string message = Encoding.UTF8.GetString(buffer[0..res.Count]).Trim();
                    if (string.IsNullOrEmpty(message))
                        continue;

                    //TODO under dosent work so this is a quick fix. think it something about empty chars at end
                    message = message.Split("\"")[1];

                    //split the message into individual arguements
                    string[] args = message.Split('|');

                    //make sure there's at least 1 argument in the message, otherwise skip
                    if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
                        continue;

                    //Check if the client want to create a new room
                    if (args[0].Equals("CREATE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //gamecontroller lobbyservice create call
                    }

                    //Check if message
                    if (args[0].Equals("MESSAGE", StringComparison.CurrentCultureIgnoreCase))
                    {
                    }

                    //if not, treat all requests as a room request
                    IRoom room;
                    if (gameController.RoomManager.Rooms.TryGetValue(args[0], out room))
                    {
                        //check nulls
                        if (!string.IsNullOrEmpty(args[1]))
                        {
                            //sort the message action types, join/leave/room message and add/remove/send data downwards
                            switch (args[1].ToUpperInvariant())
                            {
                                case "JOIN":
                                    if (gameController.Players.TryGetValue(new Player(playerId),
                                        out IPlayer playerData))
                                    {
                                        if (room.PlayerCanJoinRoom(playerData))
                                        {
                                            // Overwrite socket
                                            playerData.Socket = socket;
                                            await gameController.RoomManager.AddPlayer(playerData, room.RoomID)
                                                .ConfigureAwait(false);
                                            if (room is ILobby l)
                                            {
                                                l.PlayerReadyState[playerData] = false;
                                                await SendMessageAsync(l).ConfigureAwait(false);
                                            }
                                        }
                                        else
                                        {
                                            if (!(room is IChatRoom))
                                                await SendSimpleMessageAsync(playerData,
                                                        new PlayerAccess(false, playerId))
                                                    .ConfigureAwait(false);
                                        }
                                    }

                                    break;
                                case "LEAVE":
                                    await gameController.RoomManager.RemovePlayer(new Player(playerId), room.RoomID)
                                        .ConfigureAwait(false);
                                    await SendMessageAsync(room).ConfigureAwait(false);
                                    break;
                                default:
                                    gameController.RoomManager.Rooms[args[0]]
                                        .ReceiveMessage(new RoomMessage(playerId, args[1], args[2..]));
                                    break;
                            }
                        }
                    }
                }

                res = await socket.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None)
                    .ConfigureAwait(false);
            }

            //socket closed, remove player from rooms and disconnect socket
            ICollection<IRoom> rooms;
            //create lookup player
            IPlayer player = new Player(playerId);
            //get rooms that the player is part of
            if (gameController.RoomManager.PlayerRooms != null &&
                gameController.RoomManager.PlayerRooms.TryGetValue(player, out rooms) &&
                rooms != null)
            {
                //remove the player from all associated rooms
                foreach (var room in rooms.ToList())
                {
                    await gameController.RoomManager.RemovePlayer(player, room.RoomID).ConfigureAwait(false);
                    await SendMessageAsync(room).ConfigureAwait(false);
                }
            }

            //close socket
            await socket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None)
                .ConfigureAwait(false);
            //dispose the socket
            socket.Dispose();
        }

        private async Task SendSimpleMessageAsync(IPlayer player, object obj)
        {
            var encoded =
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(obj));
            var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await player.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        private async Task SendMessageAsync(IRoom room)
        {
            if (room is ILobby lobby)
            {
                var playerDatas = new IPlayerData[room.Players.Count];
                for (int i = 0; i < room.Players.Count; i++)
                {
                    var playerInformation = room.Players.ToArray()[i];
                    bool readyState = false;
                    if (lobby.PlayerReadyState.ContainsKey(playerInformation))
                    {
                        readyState = lobby.PlayerReadyState[playerInformation];
                    }

                    playerDatas[i] = new PlayerData(playerInformation.PlayerId,
                        playerInformation.Name, readyState);
                }

                //Loop all players and tell a player left
                foreach (var roomPlayer in room.Players)
                {
                    var encoded =
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(new LobbyData(playerDatas,
                                lobby.GameType,
                                lobby.RoomID, lobby.Name,
                                lobby.MaxPlayersNeededToStart,
                                playerDatas.Length, false)));
                    var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                    await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                            CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Sends updated room data to the api
        /// </summary>
        /// <param name="args">The room arguments of the room which state has been altered</param>
        public async void HandleNewRoomStateAsync(RoomArgs args)
        {
            //check nulls
            if (args == null || args.Room == null)
                throw new ArgumentNullException(nameof(args));

            //create a new http request
            var request = WebRequest.CreateHttp(new Uri(baseUrl + "game"));
            request.ContentType = "application/json";
            request.Timeout = 10000;
            request.Headers.Add("Authorization", "Bearer " + playerJwtTokenModel.JwtToken);

            //temporary payload object
            object payload = null;
            //check if we're dealing with lobby data
            if (args.Room is ILobby lobby)
            {
                //TODO: add check for private lobby

                //switch on the type that the data is being modified
                switch (args.ActionType)
                {
                    case RoomActionType.DELETE:

                        try
                        {
                            request = WebRequest.CreateHttp(new Uri(baseUrl + "game/" + lobby.RoomID));
                            request.Headers.Add("Authorization", "Bearer " + playerJwtTokenModel.JwtToken);
                            request.Method = "DELETE";
                            request.GetResponse();
                        }
                        catch (WebException e)
                        {
                            if (e.Response != null)
                            {
                                using (var errorResponse = (HttpWebResponse) e.Response)
                                {
                                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                                    {
                                        string error = reader.ReadToEnd();
                                        Console.WriteLine("1:" + lobby.RoomID + ": " + error);
                                    }
                                }
                            }
                        }

                        break;
                    case RoomActionType.UPDATE:
                        request.Method = "PUT";
                        payload =
                            new GameRoomData(
                                lobby.GameType,
                                lobby.RoomID,
                                string.IsNullOrWhiteSpace(lobby.Name) ? string.Empty : lobby.Name,
                                lobby.MaxPlayersNeededToStart,
                                lobby.Players.Count);
                        break;
                    case RoomActionType.CREATE:
                        lobby.OnTimerStart += gameController.HandleGameTimerStartEvent;
                        request.Method = "POST";
                        payload =
                            new GameRoomData(
                                lobby.GameType,
                                lobby.RoomID,
                                string.IsNullOrWhiteSpace(lobby.Name) ? string.Empty : lobby.Name,
                                lobby.MaxPlayersNeededToStart,
                                0);
                        break;
                    default:
                        break;
                }
            }
            //TODO: add other room subtypes

            //make sure the payload has been set
            if (payload == null)
                return;

            //get bytes of the json representation of the payload object and set the payload size
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            request.ContentLength = bytes.Length;

            //write&flush content to the uri endpoint
            using (Stream s = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await s.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                await s.FlushAsync().ConfigureAwait(false);

                //close stream
                s.Close();
            }

            //TODO Need to get response else api do not receive data
            try
            {
                request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse) e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            Console.WriteLine("1:" + args.ActionType + ": " + error);
                        }
                    }
                }
            }
        }
    }
}

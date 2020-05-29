using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Data.Models.Games;

namespace WebsocketGameServer.Data.Game.Playable
{
    public class LudoStandard : IGame
    {
        /// <summary>
        /// Ludo pawn piece data
        /// </summary>
        private class LudoPawn
        {
            public int Id { get; set; }
            public long Owner { get; set; }
            public int Position { get; set; }
        }

        /// <summary>
        /// Tile data
        /// </summary>
        private class LudoTile
        {
            public int Type { get; set; }
            public int Index { get; set; }
        }

        /// <summary>
        /// Tile type data
        /// </summary>
        private enum TileType
        {
            NONE = 0,
            STAR = 1,
            GLOBE = 2,
            GOAL_ENTRANCE = 4
        }

        //5 is done
        private IDictionary<LudoPawn, int> goalRow;

        //owner to owned map
        private IDictionary<long, LudoPawn[]> pawns;

        //the ludo tile map
        private LudoTile[] tileMap;

        //the tile that each player will move their pawns to when they get out of home
        private IDictionary<long, int> playerHomePads;

        //the tile that correspons to each players goal entrance
        private IDictionary<long, int> goalEntrances;

        public LudoStandard(int gameType, float turnTimeout, string roomID, HashSet<IPlayer> players)
        {
            if (players == null)
                throw new ArgumentNullException(nameof(players));

            GameType = gameType;
            TurnTimeout = turnTimeout;
            RoomID = roomID;
            Players = players;

            long[] playerIds = players.Select(p => p.PlayerId).ToArray();
            TurnQueue = new LinkedList<long>(playerIds);

            playerHomePads = new Dictionary<long, int>(players.Count);
            goalEntrances = new Dictionary<long, int>(players.Count);
            pawns = new Dictionary<long, LudoPawn[]>(players.Count);
            goalRow = new Dictionary<LudoPawn, int>();

            GenerateMap();
            Initialize(players);
        }

        /// <summary>
        /// Initialize message to clients
        /// </summary>
        /// <param name="players"></param>
        private async void Initialize(HashSet<IPlayer> players)
        {
            var args = new object[2];
            var playerDataSimples = new Dictionary<int, PlayerDataSimple>();

            int count = 0;
            foreach (var player in players)
            {
                playerDataSimples.Add(count, new PlayerDataSimple(player.PlayerId, player.Name));
                count++;
            }

            args[0] = playerDataSimples;

            foreach (var roomPlayer in Players)
            {
                args[1] = roomPlayer.PlayerId;
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new GameMessage(RoomID, "INIT", args)));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates a fresh ludo map
        /// </summary>
        private void GenerateMap()
        {
            tileMap = new LudoTile[52];

            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new LudoTile()
                {
                    Index = i,
                    Type = 0
                };
            }

            var players = Players.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                int pawnId, goalIndex;
                if (i == 0)
                {
                    pawnId = 0;
                    goalIndex = 0;
                }
                else
                {
                    pawnId = i * 4;

                    //Check only 2 players are playing skip green and spawn second player at orange
                    if (players.Length == 2)
                    {
                        goalIndex = (i + 1) * 13;
                    }
                    else
                    {
                        goalIndex = i * 13;
                    }
                }

                pawns.Add(players[i].PlayerId, new LudoPawn[]
                {
                    new LudoPawn()
                    {
                        Id = pawnId,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn()
                    {
                        Id = pawnId + 1,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn()
                    {
                        Id = pawnId + 2,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn()
                    {
                        Id = pawnId + 3,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                });

                goalEntrances.Add(players[i].PlayerId, goalIndex);
                playerHomePads.Add(players[i].PlayerId, goalIndex + 2);
            }
        }

        public int GameType { get; private set; }
        public long CurrentPlayerTurn { get; private set; }
        public LinkedList<long> TurnQueue { get; private set; }
        public float TurnTimeout { get; private set; }
        public DateTime StartTime { get; private set; }
        public string RoomID { get; private set; }
        public HashSet<IPlayer> Players { get; private set; }
        public event IGame.GameStateHandler GameStateChanged;

        private int Roll { get; set; }
        private int RollAttempts { get; set; }

        public async void AdvanceTurn()
        {
            var turn = TurnQueue.Find(CurrentPlayerTurn);
            if (turn.Next == null)
                CurrentPlayerTurn = TurnQueue.First.Value;
            else
                CurrentPlayerTurn = turn.Next.Value;

            RollAttempts = 0;
            var args = new object[1];
            args[0] = CurrentPlayerTurn;
            await SendMessageAsync(new GameMessage(RoomID, "NEXTTURN", args))
                .ConfigureAwait(false);
        }

        private int RollDice()
        {
            return new Random().Next(1, 7);
        }

        public IDictionary<IPlayer, float> End()
        {
            throw new NotImplementedException();
        }

        public bool PlayerCanJoinRoom(IPlayer player) => false;

        public async void ReceiveMessage(IRoomMessage message)
        {
            //Check parameters
            if (message == null || string.IsNullOrEmpty(message.Action) || message.PlayerId == 0 ||
                message.Args == null)
            {
                return;
            }

            //Check who's turn it's currently is
            if (CurrentPlayerTurn == message.PlayerId)
            {
                var action = message.Action.ToUpperInvariant();
                object[] args;
                switch (action)
                {
                    case "ROLL":
                        args = new object[2];
                        //Check have many rolls the player have
                        short totalRollAmount = RollAmount(message.PlayerId);
                        //Check if the player have any tries left
                        if (RollAttempts < totalRollAmount)
                        {
                            RollAttempts += 1;

                            //All pieces in start zone
                            if (totalRollAmount > 1)
                            {
                                //Need to role a 6 to get a piece out of starting zone
                                if (RollDice() == 6)
                                {
                                    //Move a random piece from bench to board
                                    var pawn = pawns[message.PlayerId].FirstOrDefault(a => a.Position == -1);
                                    if (pawn != null)
                                    {
                                        var moveArgs = new object[2];
                                        pawn.Position = playerHomePads[message.PlayerId];
                                        moveArgs[0] = pawn.Id;
                                        moveArgs[1] = pawn.Position;
                                        await SendMessageAsync(new GameMessage(RoomID, action, moveArgs))
                                            .ConfigureAwait(false);
                                    }

                                    AdvanceTurn();
                                }

                                if (RollAttempts == 3)
                                {
                                    //No roll left
                                    AdvanceTurn();
                                }
                            }
                            else
                            {
                                //Normal roll
                                Roll = RollDice();
                                args[0] = message.PlayerId;
                                args[1] = Roll;
                                await SendMessageAsync(new GameMessage(RoomID, action, args)).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            //TODO should not be possible to hit this
                            throw new NotImplementedException();
                        }

                        break;
                    case "MOVE":
                        if (Roll == 0)
                            return;

                        RollAttempts = 0;
                        args = new object[2];
                        //Check parameters
                        if (message.Args[0] is int pawnId && Roll != 0)
                        {
                            //Find the pawn the player is trying to move
                            var playerPawn = pawns[message.PlayerId][pawnId];

                            //Calculate new position for pawn
                            int newPosition = CalculateNewPosition(playerPawn.Position + Roll);


                            //Pawn move inside goal zone
                            if (playerPawn.Position == -2)
                            {
                                goalRow[playerPawn] = goalRow[playerPawn] + Roll;
                                AdvanceTurn();
                                return;
                            }

                            //Pawn hit goal zone
                            if (playerPawn.Position + Roll > playerHomePads[message.PlayerId] &&
                                playerPawn.Position <= playerHomePads[message.PlayerId])
                            {
                                //Hit the star before own goal so jump to end
                                if (newPosition == playerHomePads[message.PlayerId])
                                {
                                    goalRow.Add(playerPawn, 5);
                                    playerPawn.Position = -2;
                                }
                                else
                                {
                                    var goalPosition = newPosition - playerHomePads[message.PlayerId];
                                    goalRow.Add(playerPawn, goalPosition);
                                }

                                AdvanceTurn();
                                return;
                            }

                            //Check if pawn lands on star then jump to next star
                            if ((tileMap[newPosition].Type & (int) TileType.STAR) == (int) TileType.STAR)
                            {
                                newPosition = JumpNextStar(newPosition);
                            }

                            //Get a list of pawns on the same position
                            var pawnsOnPosition = PawnsOnPosition(newPosition);

                            args[2] = newPosition;
                            playerPawn.Position = newPosition;

                            //Loop other pawns if there are any
                            foreach (var otherPawn in pawnsOnPosition)
                            {
                                //Check if other player's pawns
                                if (otherPawn.Owner != message.PlayerId)
                                {
                                    //If the player's pawn land on a globe where a other pawn is already standing it kill itself.
                                    if ((tileMap[newPosition].Type & (int) TileType.GLOBE) == (int) TileType.GLOBE)
                                    {
                                        //Suicide
                                        args[1] = -1;
                                        playerPawn.Position = -1;
                                    }
                                    else
                                    {
                                        //KILL otherPawn
                                        var argsKill = new object[3];
                                        argsKill[0] = otherPawn.Id;
                                        argsKill[1] = -1;
                                        otherPawn.Position = -1;
                                        await SendMessageAsync(new GameMessage(RoomID, action, argsKill))
                                            .ConfigureAwait(false);
                                    }
                                }
                            }

                            //Clear roll
                            Roll = 0;
                            args[0] = pawnId;
                            await SendMessageAsync(new GameMessage(RoomID, action, args)).ConfigureAwait(false);
                            AdvanceTurn();
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private async Task SendMessageAsync(GameMessage gameMessage)
        {
            foreach (var roomPlayer in Players)
            {
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(gameMessage));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        private IEnumerable<LudoPawn> PawnsOnPosition(int position)
        {
            foreach (var pawn in pawns)
            {
                return pawn.Value.Where(a => a.Position == position);
            }

            return null;
        }

        private int CalculateNewPosition(int position)
        {
            if (position > 51)
            {
                position -= 52;
            }

            return position;
        }

        private int JumpNextStar(int position)
        {
            //Loop all tileMap
            foreach (var ludoTile in tileMap)
            {
                //Check if field is star and after if it after current position
                if ((tileMap[ludoTile.Index].Type & (int) TileType.STAR) == (int) TileType.STAR &&
                    position < ludoTile.Index)
                {
                    return ludoTile.Index;
                }
            }

            //If no stars is found after the index it's because it was the last star before index starts over
            return 0;
        }

        private short RollAmount(in long messagePlayerId)
        {
            //Check Amount of pawns not in start zone
            var pawnsNotInStartZone = pawns[messagePlayerId].Where(a => a.Position != -1);
            if (pawnsNotInStartZone.Any())
            {
                return 1;
            }

            return 3;
        }

        public void Start()
        {
            if (TurnQueue.First != null)
                CurrentPlayerTurn = TurnQueue.First.Value;
            //create board
            //assign player 'teams'
            //grant player turn
        }
    }
}

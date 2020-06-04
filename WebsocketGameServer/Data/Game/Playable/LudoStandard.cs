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
            public int TeamId { get; set; }
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
            string args = "";
            foreach (var player in players)
            {
                args += Environment.NewLine + player.PlayerId + "|" + player.Name;
            }

            foreach (var roomPlayer in Players)
            {
                var encoded =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new GameMessageUnity(RoomID, "INIT", roomPlayer.PlayerId + args)));
                var buffers = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await roomPlayer.Socket.SendAsync(buffers, WebSocketMessageType.Text, true,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }

            Start();
        }

        /// <summary>
        /// Generates a fresh ludo map
        /// </summary>
        private void GenerateMap()
        {
            tileMap = new LudoTile[52];

            for (int i = 0; i < tileMap.Length; i++)
            {
                int type = 0;

                //Star fields
                if (i == 0 || i == 7 || i == 13 || i == 20 || i == 26 || i == 33 || i == 39 || i == 46)
                {
                    type = 1;
                }

                //Globe fields
                if (i == 2 || i == 10 || i == 15 || i == 23 || i == 28 || i == 36 || i == 41 || i == 49)
                {
                    type = 2;
                }

                tileMap[i] = new LudoTile()
                {
                    Index = i,
                    Type = type
                };
            }

            var players = Players.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                int pawnId, goalIndex, teamId;
                if (i == 0)
                {
                    pawnId = 0;
                    goalIndex = 0;
                    teamId = 0;
                }
                else
                {
                    pawnId = i * 4;

                    //Check only 2 players are playing skip green and spawn second player at orange
                    if (players.Length == 2)
                    {
                        goalIndex = (i + 1) * 13;
                        teamId = 2;
                    }
                    else
                    {
                        teamId = i;
                        goalIndex = i * 13;
                    }
                }

                pawns.Add(players[i].PlayerId, new LudoPawn[]
                {
                    new LudoPawn
                    {
                        Id = pawnId,
                        Owner = players[i].PlayerId,
                        Position = -1,
                        TeamId = teamId
                    },
                    new LudoPawn
                    {
                        Id = pawnId + 1,
                        Owner = players[i].PlayerId,
                        Position = -1,
                        TeamId = teamId
                    },
                    new LudoPawn
                    {
                        Id = pawnId + 2,
                        Owner = players[i].PlayerId,
                        Position = -1,
                        TeamId = teamId
                    },
                    new LudoPawn
                    {
                        Id = pawnId + 3,
                        Owner = players[i].PlayerId,
                        Position = -1,
                        TeamId = teamId
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

            Roll = 0;
            RollAttempts = 0;
            await SendMessageAsync(new GameMessageUnity(RoomID, "NEXTTURN", CurrentPlayerTurn.ToString()))
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
                        //Check if player already have roled
                        if (Roll == 0)
                        {
                            //Check have many rolls the player have
                            short totalRollAmount = RollAmount(message.PlayerId);
                            //Check if the player have any tries left
                            if (RollAttempts < totalRollAmount)
                            {
                                RollAttempts += 1;

                                //All pieces in start zone
                                if (totalRollAmount > 1)
                                {
                                    var RollAmount = RollDice();
                                    //Need to role a 6 to get a piece out of starting zone
                                    if (RollAmount == 6)
                                    {
                                        //Move a random piece from bench to board
                                        var pawn = pawns[message.PlayerId].FirstOrDefault(a => a.Position == -1);
                                        if (pawn != null)
                                        {
                                            pawn.Position = playerHomePads[message.PlayerId];
                                            await SendMessageAsync(new GameMessageUnity(RoomID, "MOVE",
                                                    pawn.Id + "|" + pawn.Position))
                                                .ConfigureAwait(false);

                                            var pawnsOnPosition = PawnsOnPosition(pawn.Position).ToList();
                                            foreach (var ludoPawn in pawnsOnPosition.Where(a =>
                                                a.Owner != message.PlayerId))
                                            {
                                                await SendMessageAsync(new GameMessageUnity(RoomID, "MOVE",
                                                        ludoPawn.Id + "|-1"))
                                                    .ConfigureAwait(false);
                                            }
                                        }

                                        AdvanceTurn();
                                        return;
                                    }

                                    if (RollAttempts == 3)
                                    {
                                        //No roll left
                                        AdvanceTurn();
                                    }

                                    await SendMessageAsync(new GameMessageUnity(RoomID, "ROLL",
                                            RollAmount.ToString()))
                                        .ConfigureAwait(false);
                                }
                                else
                                {
                                    //Normal roll
                                    Roll = RollDice();
                                    await SendMessageAsync(new GameMessageUnity(RoomID, action, Roll.ToString()))
                                        .ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                //TODO should not be possible to hit this
                                throw new NotImplementedException();
                            }
                        }

                        break;
                    case "MOVE":
                        if (Roll == 0)
                            return;

                        RollAttempts = 0;

                        //Check parameters
                        if (int.TryParse(message.Args[0].ToString(), out int pawnId) && Roll != 0)
                        {
                            //Find the pawn the player is trying to move
                            var playerPawn = pawns[message.PlayerId].Single(a => a.Id == pawnId);

                            //Check if pawn is done
                            if (playerPawn.Position == -2)
                                return;

                            //Check if pawn is in start position and roll a 6
                            if (playerPawn.Position == -1)
                            {
                                if (Roll == 6)
                                {
                                    playerPawn.Position = playerHomePads[message.PlayerId];
                                    await SendMessageAsync(new GameMessageUnity(RoomID, action,
                                        playerPawn.Id + "|" + playerPawn.Position)).ConfigureAwait(false);
                                    AdvanceTurn();
                                    return;
                                }
                                else
                                {
                                    //Need to role a 6 before getting 1 more pawn out of start
                                    return;
                                }
                            }

                            var playerGoalEntrance = goalEntrances[message.PlayerId];
                            var playerGoal = 57 + (playerPawn.TeamId * 5);
                            var playerPosition = playerPawn.Position;
                            var newPositionRaw = playerPawn.Position + Roll;


                            if (playerPawn.Position > 51)
                            {
                                //Inside goal row
                                if (newPositionRaw == playerGoal)
                                {
                                    await PawnToGoal(playerPawn).ConfigureAwait(false);
                                }
                                else
                                {
                                    //Check if higher
                                    if (newPositionRaw > playerGoal)
                                    {
                                        var difference = newPositionRaw - playerGoal;
                                        playerPawn.Position = playerGoal - difference;
                                        //playerPawn.Position = newPositionRaw - playerGoal;
                                    }
                                    else
                                    {
                                        playerPawn.Position = newPositionRaw;
                                    }

                                    await SendMessageAsync(new GameMessageUnity(RoomID, action,
                                        playerPawn.Id + "|" + playerPawn.Position)).ConfigureAwait(false);
                                    AdvanceTurn();
                                }

                                return;
                            }
                            else
                            {
                                if (newPositionRaw > 51)
                                {
                                    playerPosition = 0;
                                    newPositionRaw = playerPawn.Position + Roll - 52;
                                }

                                if ((newPositionRaw >= playerGoalEntrance && playerGoalEntrance >= playerPosition))
                                {
                                    //Check if land on start
                                    if (newPositionRaw == playerGoalEntrance)
                                    {
                                        await PawnToGoal(playerPawn).ConfigureAwait(false);
                                        return;
                                    }

                                    if (playerPosition == 0)
                                    {
                                        playerPawn.Position = (playerGoal - 6) + newPositionRaw;
                                    }
                                    else
                                    {
                                        playerPawn.Position =
                                            (playerGoal - 6) +
                                            (Roll - (playerGoalEntrance - playerPawn.Position));
                                    }

                                    if (playerPawn.Position == playerGoal)
                                    {
                                        await PawnToGoal(playerPawn).ConfigureAwait(false);
                                        return;
                                    }

                                    await SendMessageAsync(new GameMessageUnity(RoomID, action,
                                        playerPawn.Id + "|" + playerPawn.Position)).ConfigureAwait(false);
                                    AdvanceTurn();
                                    return;
                                }
                            }

                            //Calculate new position for pawn
                            int newPosition = CalculateNewPosition(playerPawn.Position + Roll);

                            //Check if pawn lands on star then jump to next star
                            if ((tileMap[newPosition].Type & (int) TileType.STAR) == (int) TileType.STAR)
                            {
                                newPosition = JumpNextStar(newPosition);
                            }

                            //Get a list of pawns on the same position
                            var pawnsOnPosition = PawnsOnPosition(newPosition).ToList();

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
                                        playerPawn.Position = -1;
                                    }
                                    else
                                    {
                                        //Check if there is multiple pawns on the position
                                        if (pawnsOnPosition.Count > 1)
                                        {
                                            playerPawn.Position = -1;
                                        }
                                        else
                                        {
                                            //KILL otherPawn
                                            otherPawn.Position = -1;
                                            await SendMessageAsync(new GameMessageUnity(RoomID, action,
                                                    otherPawn.Id + "|" + "-1"))
                                                .ConfigureAwait(false);
                                        }
                                    }
                                }
                            }

                            await SendMessageAsync(new GameMessageUnity(RoomID, action,
                                playerPawn.Id + "|" + playerPawn.Position)).ConfigureAwait(false);
                            AdvanceTurn();
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private async Task PawnToGoal(LudoPawn playerPawn)
        {
            playerPawn.Position = -2;
            //SEND MESSAGE TO UNITY
            await SendMessageAsync(new GameMessageUnity(RoomID, "GOAL",
                    playerPawn.Id.ToString()))
                .ConfigureAwait(false);
            AdvanceTurn();
        }

        private async Task SendMessageAsync(GameMessageUnity gameMessage)
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
            List<LudoPawn> pawnsFound = new List<LudoPawn>();
            foreach (var pawn in pawns)
            {
                pawnsFound.AddRange(pawn.Value.Where(a => a.Position == position));
            }

            return pawnsFound;
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
            var pawnsNotInStartZone = pawns[messagePlayerId].Where(a => a.Position != -1 && a.Position != -2);
            if (pawnsNotInStartZone.Any())
            {
                return 1;
            }

            return 3;
        }

        public async void Start()
        {
            if (TurnQueue.First != null)
                CurrentPlayerTurn = TurnQueue.First.Value;

            await SendMessageAsync(new GameMessageUnity(RoomID, "NEXTTURN", CurrentPlayerTurn.ToString()))
                .ConfigureAwait(false);
        }
    }
}

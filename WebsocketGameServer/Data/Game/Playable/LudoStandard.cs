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
using WebsocketGameServer.Data.Models.Rooms;

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

            GenerateMap();
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
                    goalIndex = i * 13;
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

        public void AdvanceTurn()
        {
            var turn = TurnQueue.Find(CurrentPlayerTurn);
            if (turn.Next == null)
                CurrentPlayerTurn = TurnQueue.First.Value;
            else
                CurrentPlayerTurn = turn.Next.Value;
        }

        public int RollDice()
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
                        args = new object[1];
                        Roll = RollDice();
                        args[0] = Roll;
                        await SendMessageAsync(new GameMessage(RoomID, action, args)).ConfigureAwait(false);
                        break;
                    case "MOVE":
                        args = new object[2];
                        //Check parameters
                        if (message.Args[0] is int pawnId && Roll != 0)
                        {
                            //Find player pawn
                            var playerPawn = pawns[message.PlayerId][pawnId];

                            //Calculate new position for pawn
                            int newPosition = CalculateNewPosition(playerPawn.Position + Roll);

                            //TODO GOAL ROW
                            //if (newPosition > playerHomePads[message.PlayerId] && playerPawn.Position < playerHomePads[message.PlayerId])

                            //Check if pawn lands on star then jump to next start
                            if ((tileMap[newPosition].Type & (int) TileType.STAR) == (int) TileType.STAR)
                            {
                                newPosition = JumpNextStar(newPosition);
                            }

                            //Check if there are other pawns on the same potion
                            var pawnsOnPosition = PawnsOnPosition(newPosition);


                            args[1] = newPosition;
                            playerPawn.Position = newPosition;

                            //Loop other pawns if there are any
                            foreach (var otherPawn in pawnsOnPosition)
                            {
                                //Check if other player's pawns
                                if (otherPawn.Owner != message.PlayerId)
                                {
                                    //If pawn lands on a globus it kill itself.
                                    if ((tileMap[newPosition].Type & (int) TileType.GLOBE) == (int) TileType.GLOBE)
                                    {
                                        //Suicide
                                        args[1] = -1;
                                        playerPawn.Position = -1;
                                    }
                                    else
                                    {
                                        //KILL otherPawn
                                        var argsKill = new object[2];
                                        argsKill[0] = otherPawn.Owner + "|" + otherPawn.Id;
                                        argsKill[1] = -1;
                                        otherPawn.Position = -1;
                                        await SendMessageAsync(new GameMessage(RoomID, action, argsKill))
                                            .ConfigureAwait(false);
                                    }
                                }
                            }

                            //Clear roll
                            Roll = 0;
                            args[0] = message.PlayerId + "|" + pawnId;
                            await SendMessageAsync(new GameMessage(RoomID, action, args)).ConfigureAwait(false);
                            //NEXT TURN HOW THE FUCK
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

        public void Start()
        {
            CurrentPlayerTurn = TurnQueue.First.Value;
            //create board
            //assign player 'teams'
            //grant player turn
        }
    }
}

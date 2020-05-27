using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Players;
using WebsocketGameServer.Data.Game.Room;
using WebsocketGameServer.Data.Messages;

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
        private IDictionary<long, LudoTile> playerHomePads;
        //the tile that correspons to each players goal entrance
        private IDictionary<long, LudoTile> goalEntrances;

        public LudoStandard(int gameType, float turnTimeout, string roomID, HashSet<IPlayer> players)
        {
            GameType = gameType;
            TurnTimeout = turnTimeout;
            RoomID = roomID;
            Players = players;

            long[] playerIds = players.Select(p => p.PlayerId).ToArray();
            TurnQueue = new LinkedList<long>(playerIds);

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
            for (int i = 0; i < players.Count; i++)
            {
                int pawnId;
                if (i == 0)
                    pawnId = 0;
                else
                    pawnId = i * 4;

                pawns.Add(players[i].PlayerId, new LudoPawn[] {
                    new LudoPawn(){
                        Id = pawnId,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn(){
                        Id = pawnId+1,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn(){
                        Id = pawnId+2,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                    new LudoPawn(){
                        Id = pawnId+3,
                        Owner = players[i].PlayerId,
                        Position = -1
                    },
                });
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

        public void AdvanceTurn()
        {
            var turn = TurnQueue.Find(CurrentPlayerTurn);
            if (turn.Next == null)
                CurrentPlayerTurn = TurnQueue.First.Value;
            else
                CurrentPlayerTurn = turn.Next.Value;
        }

        public IDictionary<IPlayer, float> End()
        {
            throw new NotImplementedException();
        }

        public bool PlayerCanJoinRoom(IPlayer player) => false;

        public void ReceiveAction(IPlayer player, string action, object[] args)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(IRoomMessage message)
        {
            throw new NotImplementedException();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;

namespace WebsocketGameServer.Data.Game.Room
{
    /// <summary>
    /// Base game interface for defining games
    /// </summary>
    public interface IGame : IRoom
    {
        //state handling and notification
        public delegate void GameStateHandler(IGame game, GameState state);
        public event GameStateHandler GameStateChanged;

        /// <summary>
        /// The type of game that the implementing class is
        /// </summary>
        public int GameType { get; }

        /// <summary>
        /// The id of the player which is currently playing out their turn
        /// </summary>
        public long CurrentPlayerTurn { get; }

        /// <summary>
        /// The turn queue of the game
        /// </summary>
        public LinkedList<long> TurnQueue { get; }

        /// <summary>
        /// The time each turn can take(max time before turn being skipped) in seconds
        /// </summary>
        public float TurnTimeout { get; set; }

        /// <summary>
        /// The time that the game was started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void Start();

        /// <summary>
        /// Advances the game and grants the next player in queue their turn
        /// </summary>
        public void AdvanceTurn();

        /// <summary>
        /// Receives and processes an incoming player action
        /// </summary>
        /// <param name="player">The player performing the action</param>
        /// <param name="action">The action being performed</param>
        /// <param name="args">The arguments, if any, provided with the action</param>
        public void ReceiveAction(IPlayer player, string action, object[] args);

        /// <summary>
        /// Ends the game
        /// </summary>
        /// <returns>The player scores</returns>
        public IDictionary<IPlayer, float> End();
    }

    public enum GameState
    {
        STARTING,
        RUNNING,
        ENDING
    }
}

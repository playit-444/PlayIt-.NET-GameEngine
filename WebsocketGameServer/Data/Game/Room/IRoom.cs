﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsocketGameServer.Data.Game.Player;
using WebsocketGameServer.Data.Messages;
using WebsocketGameServer.Models.Args;

namespace WebsocketGameServer.Data.Game.Room
{
    public interface IRoom
    {
        /// <summary>
        /// Delegate for defining methods capable of handling room state events
        /// </summary>
        /// <param name="args">The room arguments of the event</param>
        public delegate void RoomHandler(RoomArgs args);

        /// <summary>
        /// The event being invoked upon the room changing state
        /// </summary>
        public event RoomHandler RoomStateChanged;

        /// <summary>
        /// The randomly generated ID of the room
        /// </summary>
        string RoomID { get; }

        /// <summary>
        /// The players contained within the room
        /// </summary>
        HashSet<IPlayer> Players { get; }

        /// <summary>
        /// Determies whether a certain player can join the room
        /// </summary>
        /// <param name="player">The player that is attempting to join the room</param>
        /// <returns>Whether the player is capable of joining the room</returns>
        public Task<bool> PlayerCanJoinRoom(IPlayer player);

        public void ReceiveMessage(IRoomMessage message);
    }
}

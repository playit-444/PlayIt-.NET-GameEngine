namespace WebsocketGameServer.Data.Models.Rooms
{
    /// <summary>
    /// Room transfer data for sending a simple room state object
    /// </summary>
    public interface IRoomData
    {
        /// <summary>
        /// The ID of the room
        /// </summary>
        public string RoomID { get; }

        /// <summary>
        /// The name of the room
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The maximum amount of users the room can hold
        /// </summary>
        public int MaxUsers { get; }

        /// <summary>
        /// The amount of users currently within the room
        /// </summary>
        public int CurrentUsers { get; }

        /// <summary>
        /// Whether the room is password protected and thus private
        /// </summary>
        public bool PrivateRoom { get; }
    }
}

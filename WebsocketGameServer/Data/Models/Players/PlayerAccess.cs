namespace WebsocketGameServer.Data.Models.Players
{
    /// <summary>
    /// Check if player got permission denied for joining room fx
    /// </summary>
    public class PlayerAccess
    {
        public long PlayerId { get; private set; }
        public bool Access { get; private set; }

        public PlayerAccess(bool access, long playerId)
        {
            Access = access;
            PlayerId = playerId;
        }
    }
}

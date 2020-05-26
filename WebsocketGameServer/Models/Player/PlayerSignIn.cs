namespace WebsocketGameServer.Models.Player
{
    public class PlayerSignIn
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Ipv4 { get; set; }

        public PlayerSignIn(string userName, string password, string ipv4)
        {
            UserName = userName;
            Password = password;
            Ipv4 = ipv4;
        }
    }
}

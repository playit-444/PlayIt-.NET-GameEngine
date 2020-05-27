using System;
using Org.BouncyCastle.Security;

namespace WebsocketGameServer.Services.Generators
{
    public class HexIdGenerator : IIdentifierGenerator
    {
        readonly SecureRandom Random;

        public HexIdGenerator()
        {
            Random = new SecureRandom();
        }

        public string CreateID(in int length)
        {
            byte[] bytes = new byte[length];
            Random.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

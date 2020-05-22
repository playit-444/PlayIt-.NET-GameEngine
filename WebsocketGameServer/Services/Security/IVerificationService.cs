using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Services.Security
{
    public interface IVerificationService
    {
        public Task<bool> VerifyToken(string token);
    }
}

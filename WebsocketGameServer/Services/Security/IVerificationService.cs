using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Services.Security
{
    /// <summary>
    /// Interface for defining a service which verifies player tokens
    /// </summary>
    /// <typeparam name="T">The return type once a token has been verified</typeparam>
    public interface IVerificationService<T> where T : class
    {
        public Task<T> VerifyToken(string token);
    }
}

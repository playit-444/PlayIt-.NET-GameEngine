﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsocketGameServer.Services.Security
{
    public interface IVerificationService<T> where T : class
    {
        public Task<T> VerifyToken(string token);
    }
}

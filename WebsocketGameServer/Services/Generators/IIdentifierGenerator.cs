﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Services.Generators
{
    /// <summary>
    /// Interface capable of creating string based ids
    /// </summary>
    public interface IIdentifierGenerator
    {
        /// <summary>
        /// Creates a unique identifier in string format
        /// </summary>
        /// <param name="length">The length of the id</param>
        /// <returns>the finished id</returns>
        string CreateID(in int length);
    }
}

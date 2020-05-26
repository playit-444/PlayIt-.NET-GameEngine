using System;
using System.Threading.Tasks;

namespace WebsocketGameServer.Logging
{
    /// <summary>
    /// Base interface for logging behaviour
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Logs a message synchronously
        /// </summary>
        /// <param name="msg">the message to be logged</param>
        void Log(string msg);
        /// <summary>
        /// Logs a message asynchronously
        /// </summary>
        /// <param name="msg">the message to be logged</param>
        Task LogAsync(string msg);
    }
}

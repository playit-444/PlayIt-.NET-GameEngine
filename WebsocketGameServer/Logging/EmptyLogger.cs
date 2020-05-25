using System;
using System.Threading.Tasks;

namespace WebsocketGameServer.Logging
{
    /// <summary>
    /// Empty logging implementation to be used as a placeholder
    /// </summary>
    public class EmptyLogger : ILogger
    {
        private bool isDisposed = false;
        ~EmptyLogger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {

            }
        }

        public void Log(string msg)
        {

        }

        public async Task LogAsync(string msg)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}

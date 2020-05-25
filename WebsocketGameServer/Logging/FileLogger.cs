using System;
using System.IO;
using System.Threading.Tasks;

namespace WebsocketGameServer.Logging
{
    public class FileLogger : ILogger
    {
        public void Log(string msg)
        {
            try
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    w.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task LogAsync(string msg)
        {
            try
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    await w.WriteLineAsync(msg).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        ~FileLogger()
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
            if (disposing)
            {
                // TODO
            }
        }
    }
}

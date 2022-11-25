using Game.Server;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameService
{
    public class ConsoleStart
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleStart));

        public static void RunGameConsole()
        {
            Console.Title = "Game.Service";
            log.InfoFormat("Starting GameService...");

            GameServer.CreateInstance();
            GameServer.Instance.Start();

            while (true)
            {

            }
        }
    }
}

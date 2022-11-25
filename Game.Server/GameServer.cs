using Game.Components.Events;
using Game.Components.GameEvents;
using Game.Components.Scripts;
using Game.Network.Bases;
using Game.Network.Events;
using Game.Resources;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public class GameServer : BaseServer
    {
        /// <summary>
        /// Current GameServer instance
        /// </summary>
        public static GameServer Instance;

        /// <summary>
        /// GetLogger of current class
        /// </summary>
        public static ILog log = LogManager.GetLogger(typeof(GameServer));

        /// <summary>
        /// Create a new server instance
        /// </summary>
        /// <returns></returns>
        public static new GameServer CreateInstance()
        {
            if (Instance == null)
                Instance = new GameServer();

            return Instance;
        }

        /// <summary>
        /// Start GameServer
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Normal;

            if (!this.InitComponent("Essentials GameComponents", this.StartEssentialsComponents()))
                return false;

            if (!this.InitComponent("ConfigResource", ConfigResource.Setup()))
                return false;

            if (!this.InitComponent("LanguageResource", LanguageResource.Setup()))
                return false;

            if (!this.InitComponent("Socket", this.CreateSocket(new IPEndPoint(IPAddress.Parse(ConfigResource.GetAddress()), ConfigResource.GetPort()))))
                return false;

            GameEventMgr.Notify(ScriptEvent.Loaded);

            if (!base.Start())
            {
                log.Error("Server is not started!");
                return false;
            }

            GameEventMgr.Notify(GameServerEvent.Started, this);
            log.Warn("GameServer is now open for connections!");
            GC.Collect(GC.MaxGeneration);

            base.OnClientConnectedEvent += (object sender, OnClientConnectedHandler e) =>
            {
                log.Info($"Incoming connection from {e.GetNewConnectClient().GetConnectionAddress()}, {e.GetNewConnectClient().Guid}!");
            };

            base.OnClientDisconnectedEvent += (object sender, OnClientDisconnectedHandler e) =>
            {
                log.Info($"Disconnecting client [{e.GetDisconnectedClient().GetConnectionAddress()}][{e.GetDisconnectedClient().Guid}], received data: 0 bytes!");
            };

            base.OnMessageReceivedEvent += (object sender, OnMessageReceivedHandler e) =>
            {
                log.Info($"Received message from [{e.SourceClient.Guid}], Data: {e.MessageData}");
            };

            return true;
        }

        /// <summary>
        /// Loads essential components for the proper functioning of the server
        /// </summary>
        /// <returns></returns>
        protected bool StartEssentialsComponents()
        {
            bool isStarted = false;
            try
            {
                ScriptMgr.InsertAssembly(typeof(GameServer).Assembly);
                ScriptMgr.InsertAssembly(typeof(BaseServer).Assembly);
                
                foreach(var item in new ArrayList(ScriptMgr.Scripts))
                {
                    var assemblyItem = (item as Assembly);
                    GameEventMgr.RegisterGlobalEvents(assemblyItem, typeof(GameServerStartedEventAttribute), GameServerEvent.Started);
                    GameEventMgr.RegisterGlobalEvents(assemblyItem, typeof(GameServerStoppedEventAttribute), GameServerEvent.Stopped);
                    GameEventMgr.RegisterGlobalEvents(assemblyItem, typeof(ScriptLoadedEventAttribute), ScriptEvent.Loaded);
                    GameEventMgr.RegisterGlobalEvents(assemblyItem, typeof(ScriptUnloadedEventAttribute), ScriptEvent.Unloaded);
                }

                if (log.IsWarnEnabled)
                    log.Warn("Registering global event handlers: true!");

                isStarted = true;
            }
            catch(Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error($"StartEssentialsComponents: {ex.Message}:{ex.StackTrace}");
            }
            return isStarted;
        }

        /// <summary>
        /// Creates a new client on the game data connection socket
        /// </summary>
        /// <param name="socket">Socket that must be created assigned to client</param>
        /// <returns></returns>
        protected override BaseClient CreateClient(Socket socket)
        {
            var client = new GameClient(this, socket);
            BaseClient newClient = null;

            for(int i = 0; i < this._clients.Length; i++)
            {
                if (this._clients[i] == null)
                {
                    this._clients[i] = client;
                    newClient = (this._clients[i]);
                    break;
                }
            }

            return newClient;
        }

        /// <summary>
        /// Start components, and display messages according to their boolean results
        /// </summary>
        /// <param name="componentName">Name of the component to be started</param>
        /// <param name="state">Component status started</param>
        /// <returns></returns>
        private bool InitComponent(string componentName, bool state)
        {
            if (state && log.IsDebugEnabled)
            {
                log.DebugFormat($"Init [{componentName}], State: {state}!");
            }
            else if (!state && log.IsDebugEnabled)
            {
                log.ErrorFormat($"Init [{componentName}], Fail!");
            }
            return state;
        }
    }
}

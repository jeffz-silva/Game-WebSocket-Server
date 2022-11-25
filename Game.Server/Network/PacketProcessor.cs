using Game.Components.Events;
using Game.Components.GameEvents;
using Game.Server.Packets.Client.Attributes;
using Game.Server.Packets.Client.Interfaces;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Network
{
    public class PacketProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PacketProcessor));

        /// <summary>
        /// List of classes containing a manipulation interface
        /// </summary>
        protected static readonly Dictionary<int, IPacketHandler> _packetHandlers = new Dictionary<int, IPacketHandler>();

        /// <summary>
        /// Client processing package data
        /// </summary>
        protected GameClient _client;

        public PacketProcessor(GameClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Handling of transported data
        /// </summary>
        /// <param name="packet">transported data</param>
        public void HandlePacket(JObject packet)
        {
            try
            {
                int opcode = (int)packet.GetValue("Opcode");
                IPacketHandler packetInterface = null;

                if (!_packetHandlers.ContainsKey(opcode))
                {
                    if (log.IsWarnEnabled) log.WarnFormat($"-- WARNING -- : Opcode: {opcode} not handleable by the server!");
                    return;
                }

                packetInterface = (_packetHandlers[opcode]);

                long startHandlerTick = Environment.TickCount;
                packetInterface.HandlePacket(this._client, packet);
                long endHandlerTick = Environment.TickCount;

                long costHandlerTick = (endHandlerTick - startHandlerTick);
                if (log.IsDebugEnabled)
                    log.Debug($"Package process time {costHandlerTick}ms!");

                if (costHandlerTick <= 1000L)
                    return;

                if (log.IsWarnEnabled)
                    log.Warn($"({this._client.GetConnectionAddress()}) Handle packet Thread [{Thread.CurrentThread.ManagedThreadId}][{packetInterface}] took {costHandlerTick}ms!");
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error($"{ex.Message}:{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Running during script compilation process
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        [ScriptLoadedEvent]
        public static void OnScriptCompiled(ServerEvent ev, object sender, EventArgs args)
        {
            _packetHandlers.Clear();
            int loadedOpcdes = (SearchPacketHandlers(Assembly.GetAssembly(typeof(GameServer))));

            if (log.IsInfoEnabled)
                log.Info($"PacketProcessor: Loaded [{loadedOpcdes}] handlers from GameServer Assembly!");
        }

        /// <summary>
        /// Registers a new opcode to be manipulated
        /// </summary>
        /// <param name="opcode">Opcode identification</param>
        /// <param name="handler">Handling class</param>
        public static void RegisterPacketHandler(int opcode, IPacketHandler handler)
        {
            if (_packetHandlers.ContainsKey(opcode))
                return;

            _packetHandlers.Add(opcode, handler);
        }

        /// <summary>
        /// Search all handling classes within the server project
        /// </summary>
        /// <param name="assembly">Project Assembly</param>
        /// <returns></returns>
        protected static int SearchPacketHandlers(Assembly assembly)
        {
            int handlesOpcodes = 0;
            Type[] packetTypes = (assembly.GetTypes());

            foreach(var packetType in packetTypes)
            {
                if(packetType.IsClass && !(packetType.GetInterface("Game.Server.Packets.Client.Interfaces.IPacketHandler") == null))
                {
                    PacketHandlerAttribute[] packetAttributes = (PacketHandlerAttribute[])packetType.GetCustomAttributes(typeof(PacketHandlerAttribute), inherit: true);
                    if(packetAttributes.Length != 0)
                    {
                        RegisterPacketHandler(packetAttributes[0].Code, (IPacketHandler)Activator.CreateInstance(packetType));
                        handlesOpcodes++;
                    }
                }
            }
            return handlesOpcodes;
        }
    }
}

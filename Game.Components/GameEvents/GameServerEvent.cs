using Game.Components.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.GameEvents
{
    public class GameServerEvent : ServerEvent
    {
        public static readonly GameServerEvent Started = new GameServerEvent("Server.Started");
        public static readonly GameServerEvent Stopped = new GameServerEvent("Server.Stopped");
        public static readonly GameServerEvent WorldSave = new GameServerEvent("Server.WorldSave");

        protected GameServerEvent(string name)
          : base(name)
        {
        }
    }
}

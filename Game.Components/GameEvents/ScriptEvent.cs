using Game.Components.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.GameEvents
{
    public class ScriptEvent : ServerEvent
    {
        public static readonly ScriptEvent Loaded = new ScriptEvent("Script.Loaded");
        public static readonly ScriptEvent Unloaded = new ScriptEvent("Script.Unloaded");

        protected ScriptEvent(string name)
          : base(name)
        {
        }
    }
}

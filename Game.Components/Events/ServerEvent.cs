using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.Events
{
    public abstract class ServerEvent
    {
        protected string _eventName;
        public string Name => _eventName;

        public ServerEvent(string eventName)
        {
            _eventName = eventName;
        }

        public virtual bool isValidFor(object o) => true;
        public override string ToString() => ("DOLEvent(" + this._eventName + ")");
    }
}

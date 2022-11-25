using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.Events
{
    public delegate void ServerEventHandler(ServerEvent e, object sender, EventArgs args);
}

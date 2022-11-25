using Game.Network.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Events
{
    public class OnClientDisconnectedHandler : EventArgs
    {
        private BaseClient _disconnectedClient;

        public OnClientDisconnectedHandler(BaseClient client)
        {
            this._disconnectedClient = client;
        }

        public BaseClient GetDisconnectedClient() => _disconnectedClient;
    }
}

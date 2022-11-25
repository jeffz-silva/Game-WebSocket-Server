using Game.Network.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Events
{
    public class OnClientConnectedHandler : EventArgs
    {
        private BaseClient _connectedClient;

        public OnClientConnectedHandler(BaseClient client)
        {
            this._connectedClient = client;
        }

        public BaseClient GetNewConnectClient() => _connectedClient;
    }
}

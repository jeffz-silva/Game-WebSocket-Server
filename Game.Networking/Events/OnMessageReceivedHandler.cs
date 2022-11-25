using Game.Network.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Events
{
    public class OnMessageReceivedHandler : EventArgs
    {
        private BaseClient _sourceClient;
        private string _messageData;

        public BaseClient SourceClient => _sourceClient;
        public string MessageData => _messageData;

        public OnMessageReceivedHandler(BaseClient sourceClient, string messageData)
        {
            _sourceClient = sourceClient;
            _messageData = messageData;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Data
{
    public class NetworkProperties
    {
        //Limit of online players is max number of socket ports (65565)
        public const int MAX_PLAYERS = 10000;

        //Listening state time
        public const int SOCKET_BACK_LOG = 1000;

        //Size of information received in a handshake
        public const int HANDSHAKE_BUFFER = 4096;

        public const int DATA_BUFFFER = 8192;

        //Obfuscated handshake key
        public const string HANDSHAKE_KEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    }
}

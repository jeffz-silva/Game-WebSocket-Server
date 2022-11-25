using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Enums
{
    public enum EOpcodeType
    {
        Fragment = 0, //%x0 denotes a continuation frame
        Text = 1, //%x1 denotes a text frame
        Binary = 2, // %x2 denotes a binary frame

        // %x3-7 are reserved for further non-control frames

        ClosedConnection = 8, // %x8 denotes a connection close
        Ping = 9, // %x9 denotes a ping
        Pong = 10 //%xA denotes a pong
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Server
{
    public interface ISocketOut
    {
        void SendUserWalletConnectedState(bool isCreated);
        void SendSimpleAlert(string message);
        void SendCompletedRegister();
        void SendCompletedLogin();
    }
}

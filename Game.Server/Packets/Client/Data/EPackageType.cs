using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Client.Data
{
    public enum EPackageType
    {
        WalletConnect = 0,
        Register = 1,
        CompletedRegister = 2,
        Login = 3,
        SimpleAlert = 100
    }
}

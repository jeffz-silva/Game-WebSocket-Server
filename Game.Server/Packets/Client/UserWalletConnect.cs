using Game.Database;
using Game.Server.Packets.Client.Attributes;
using Game.Server.Packets.Client.Data;
using Game.Server.Packets.Client.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)EPackageType.WalletConnect, "Conexão com a carteira")]

    public class UserWalletConnect : IPacketHandler
    {
        public int HandlePacket(GameClient client, JObject packet)
        {
            string walletId = (string)packet.GetValue("WalletId");
            if (string.IsNullOrEmpty(walletId))
                return 0;

            client.SetConnectedWallet(walletId);
            using (GameUserData userData = new GameUserData())
            {
                if (userData.UserInfos.Where(p => p.Wallet == walletId).Any())
                {
                    client.Out.SendUserWalletConnectedState(isCreated: true);
                    return 0;
                }
                client.Out.SendUserWalletConnectedState(isCreated: false);
            }
            return 0;
        }
    }
}

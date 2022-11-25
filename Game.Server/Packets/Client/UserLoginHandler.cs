using Game.Database;
using Game.Resources;
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
    [PacketHandler((int)EPackageType.Login, "Autenticação")]
    public class UserLoginHandler : IPacketHandler
    {
        public int HandlePacket(GameClient client, JObject packet)
        {
            if (client == null)
                return 0;

            if (string.IsNullOrEmpty(client.ConnectedWallet))
            {
                client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserLoginHandler.NoWallet"));
                return 0;
            }

            string password = (string)packet.GetValue("Password");
            if (string.IsNullOrEmpty(password))
            {
                client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserLoginHandler.IsNullOrEmpty"));
                return 0;
            }

            using(GameUserData userData = new GameUserData())
            {
                if(!userData.UserInfos.Any(p => p.Wallet == client.ConnectedWallet))
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserLoginHandler.NoAnyWallet"));
                    return 0;
                }

                var userInfo = (userData.UserInfos.First(p => p.Wallet == client.ConnectedWallet));
                if (!userInfo.Password.Equals(password))
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserLoginHandler.WrongPassword"));
                    return 0;
                }
            }

            client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserLoginHandler.Success"));
            client.Out.SendCompletedLogin();
            return 0;
        }
    }
}

using Game.Database;
using Game.Database.UserDataModels;
using Game.Resources;
using Game.Server.Packets.Client.Attributes;
using Game.Server.Packets.Client.Data;
using Game.Server.Packets.Client.Interfaces;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)EPackageType.Register, "Cadastro")]

    public class UserRegisterHandler : IPacketHandler
    {
        private static ILog log = LogManager.GetLogger(typeof(UserRegisterHandler));


        public int HandlePacket(GameClient client, JObject packet)
        {
            try
            {
                string email = (string)packet.GetValue("Email");
                string password = (string)packet.GetValue("Password");
                string confirmPassword = (string)packet.GetValue("ConfirmPassword");
                string nickName = (string)packet.GetValue("NickName");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(nickName))
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.IsNullOrEmpty"));
                    return 0;
                }

                if (!email.Contains("@"))
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.ValidMail"));
                    return 0;
                }

                if ((password != confirmPassword) || (password.Length < 4 || password.Length > 16))
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.ValidPassword"));
                    return 0;
                }

                if (nickName.Length < 3 || nickName.Length > 12)
                {
                    client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.ValidNickName"));
                    return 0;
                }

                using (GameUserData userData = new GameUserData())
                {
                    if (userData.UserInfos.Any(p => p.Email == email))
                    {
                        client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.AnyMail"));
                        return 0;
                    }

                    if (userData.UserInfos.Any(p => p.Wallet == client.ConnectedWallet))
                    {
                        client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.AnyWallet"));
                        return 0;
                    }

                    if (userData.UserInfos.Any(p => p.NickName == nickName))
                    {
                        client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.AnyNickName"));
                        return 0;
                    }

                    userData.UserInfos.Add(new UserInfo
                    {
                        Wallet = client.ConnectedWallet,
                        Email = email,
                        NickName = nickName,
                        Password = password,
                        CreateDate = DateTime.Now,
                    });
                    userData.SaveChanges();
                }
                client.Out.SendSimpleAlert(LanguageResource.GetTranslation("Game.Server.Packets.Client.UserRegisterHandler.Success"));
                client.Out.SendCompletedRegister();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error($"{ex.Message}:{ex.StackTrace}:{ex.InnerException}");
            }
            return 0;
        }
    }
}

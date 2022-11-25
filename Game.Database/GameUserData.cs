using Game.Database.UserDataModels;
using Game.Resources;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Database
{
    public class GameUserData : DbContext
    {
        private static ILog log = LogManager.GetLogger(typeof(GameUserData));

        #region Data Tables

        public DbSet<UserInfo> UserInfos { get; set; }

        #endregion

        #region Configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configDatabase = ConfigResource.GetDatabaseConfig();

            string serverName = (string)(configDatabase.GetValue("server"));
            string database = (string)(configDatabase.GetValue("database"));
            string userId = (string)(configDatabase.GetValue("userid"));
            string password = (string)(configDatabase.GetValue("password"));

            if(string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                log.WarnFormat("Database is not configured!");
                return;
            }

            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseSqlServer($"Server={serverName};Database={database};User Id={userId};Password={password};TrustServerCertificate=True");
        }
        #endregion
    }
}

using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Resources
{
    public class ConfigResource
    {
        private static JObject _configData = new JObject();

        private static ILog log = LogManager.GetLogger(typeof(ConfigResource));

        /// <summary>
        /// Load configuration file
        /// </summary>
        public static bool Setup()
        {
            try
            {
                using (StreamReader reader = File.OpenText("config.json"))
                {
                    _configData = JObject.Parse(reader.ReadToEnd());
                }
                return true;
            }
            catch (Exception ex)
            {
                if(log.IsErrorEnabled)
                    log.Error($"[ConfigResource] {ex.Message}:{ex.StackTrace}");
            }
            return false;
        }

        /// <summary>
        /// Get connection address from configuration file
        /// </summary>
        /// <returns></returns>
        public static string GetAddress() => (_configData.GetValue("address") != null ? (string)_configData.GetValue("address") : "127.0.0.1");

        /// <summary>
        /// Get connection port in configuration file
        /// </summary>
        /// <returns></returns>
        public static int GetPort() => (_configData.GetValue("port") != null ? (int)_configData.GetValue("port") : 9200);

        /// <summary>
        /// Collection of database configuration data
        /// </summary>
        /// <returns></returns>
        public static JObject GetDatabaseConfig() => ((JObject)_configData.GetValue("sql_connection_user"));

        /// <summary>
        /// Collects location data from server configuration files
        /// </summary>
        /// <returns></returns>
        public static JObject GetFilesConfig() => ((JObject)_configData.GetValue("files"));

        /// <summary>
        /// Get language file location
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageFilePath() => ((string)GetFilesConfig().GetValue("language"));
    }
}

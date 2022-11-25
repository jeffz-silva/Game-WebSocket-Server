using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Resources
{
    public static class LanguageResource
    {
        private static ILog log = LogManager.GetLogger(typeof(LanguageResource));

        /// <summary>
        /// List of loaded messages
        /// </summary>
        private static Hashtable LanguageSentences;

        /// <summary>
        /// Call function to load the translated messages
        /// </summary>
        /// <returns></returns>
        public static bool Setup()
        {
            return LanguageResource.Reload();
        }

        /// <summary>
        /// Reload translated messages
        /// </summary>
        /// <returns></returns>
        public static bool Reload()
        {
            try
            {
                Hashtable tempSentences = LanguageResource.LoadLanguage();
                if (tempSentences.Count == 0)
                    return false;

                Interlocked.Exchange<Hashtable>(ref LanguageResource.LanguageSentences, tempSentences);
                return true;
            }
            catch (Exception ex)
            {
                if(log.IsErrorEnabled)
                    log.Error(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Load messages from a file
        /// </summary>
        /// <returns></returns>
        private static Hashtable LoadLanguage()
        {
            Hashtable sentences = new Hashtable();
            if (!File.Exists(ConfigResource.GetLanguageFilePath()))
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("LanguageFile is not founded!");
                return sentences;
            }

            string[] lines = File.ReadAllLines(ConfigResource.GetLanguageFilePath(), Encoding.UTF8);
            IList textList = new ArrayList(lines);
            foreach (string line in textList)
            {
                if (line.StartsWith("#") || line.IndexOf(':') < 0)
                    continue;

                string[] splitted = new string[]
                {
                    line.Substring(0, line.IndexOf(':')),
                    line.Substring(line.IndexOf(':') + 1)
                };
                splitted[1] = splitted[1].Replace("\t", "");
                sentences[splitted[0]] = splitted[1];
            }
            return sentences;
        }

        /// <summary>
        /// Collect a specific message
        /// </summary>
        /// <param name="translateId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetTranslation(string translateId, params object[] args)
        {
            string translation;

            if (!LanguageResource.LanguageSentences.ContainsKey(translateId))
                return translateId;

            translation = (string)LanguageResource.LanguageSentences[translateId];
            try
            {
                translation = string.Format(translation, args);
            }
            catch (Exception ex)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat($"[LanguageMgr] Parameters number error, ID: {translateId} (Arg count={args.Length})", ex.Message);
            }
            return (translation ?? translateId);
        }
    }
}

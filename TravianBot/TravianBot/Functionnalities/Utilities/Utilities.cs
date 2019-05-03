using System.IO;
using System.Text;

using Newtonsoft.Json;
using TravianBot.Functionnalities.Data;

namespace TravianBot.Functionnalities.Utilities
{
    static class Utilities
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
              
        static public AttackTargets GetDataJson(string path)
        {
            log.Info($"Opening {path}");
            string rawJson = ReadFile(path);

            AttackTargets dataObject = JsonConvert.DeserializeObject<AttackTargets>(rawJson);

            return dataObject;
        }

        static public string ReadFile(string path)
        {            
            return File.ReadAllText(path, Encoding.Default);
        }
    }
}

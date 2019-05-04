using System;
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

        static public int TimeToMs(string time)
        {
            string[] times = time.Replace("in ", "").Replace(" hrs.", "").Split(':');

            return (Int32.Parse(times[0]) * 60 * 60 + Int32.Parse(times[1]) * 60 + Int32.Parse(times[2])) * 1000;
        }
    }
}

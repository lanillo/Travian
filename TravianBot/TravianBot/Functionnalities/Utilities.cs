using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace TravianBot.Functionnalities
{
    static class Utilities
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
              
        [TestMethod]
        static public Data GetDataJson(string path)
        {
            log.Info($"Opening {path}");
            string data = ReadFile(path);

            Data dataObject = JsonConvert.DeserializeObject<Data>(data);

            return dataObject;
        }

        static public string ReadFile(string path)
        {            
            return File.ReadAllText(path, Encoding.Default);
        }
    }
}

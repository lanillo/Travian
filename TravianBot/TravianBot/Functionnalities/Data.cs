using Newtonsoft.Json;
using System.Collections.Generic;

namespace TravianBot.Functionnalities
{
    class Data
    {
        [JsonProperty("Oases")]
        public List<Oases> Oases { get; set; }

        [JsonProperty("Villages")]
        public List<Village> Villages { get; set; }
    }

    class Oases
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("X")]
        public int X { get; set; }

        [JsonProperty("Y")]
        public int Y { get; set; }

        public bool IsAttacked { get; set; }

        public int TimeUntilAttackLands { get; set; }
    }

    class Village
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("X")]
        public int X { get; set; }

        [JsonProperty("Y")]
        public int Y { get; set; }

        public bool IsAttacked { get; set; }
    }
}

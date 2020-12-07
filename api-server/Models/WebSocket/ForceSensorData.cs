using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace api_server
{
    public class ForceSensorData
    {
        [JsonProperty("fx")]
        public float Fx { get; set; }

        [JsonProperty("fy")]
        public float Fy { get; set; }

        [JsonProperty("fz")]
        public float Fz { get; set; }

        [JsonProperty("mx")]
        public float Mx { get; set; }

        [JsonProperty("my")]
        public float My { get; set; }

        [JsonProperty("mz")]
        public float Mz { get; set; }
    }

}

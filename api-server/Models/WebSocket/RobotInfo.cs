using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace api_server
{
    public class Position
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }

        [JsonProperty("role")]
        public float Role { get; set; }

        [JsonProperty("pitch")]
        public float Pitch { get; set; }

        [JsonProperty("yaw")]
        public float Yaw { get; set; }
    }

    public class RobotInfo
    {
        [JsonProperty("tcp")]
        public Position Tcp { get; set; }

        [JsonProperty("time")]
        public Int64 Time { get; set; }
    }

}

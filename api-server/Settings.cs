using System;
using System.Text;
using Newtonsoft.Json;

public class Settings
{
    [JsonProperty("cfd-ip")]
    public string CfdIP { get; set; } = "127.0.0.1";

    [JsonProperty("sensor-ip")]
    public string SensorIP { get; set; } = "192.168.0.200";

    [JsonProperty("sensor-com")]
    public string SensorCom { get; set; } = null;

    [JsonProperty("cfd-port")]
    public int CfdPort { get; set; } = 9876;


}
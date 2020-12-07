using System;
using System.Text;
using System.IO;

public static class AppConst
{
    public const int WebSocketReceiveBufferSize = 4 * 1024;

    public static readonly Encoding NON_BOM_UTF8_ENCORDING = new UTF8Encoding(false);

    public static readonly string SETTING_FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "settings", "setting.json");
}
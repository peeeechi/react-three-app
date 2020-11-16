using System;
using System.Text;

public static class AppConst
{
    public const int WebSocketReceiveBufferSize = 4 * 1024;

    public static readonly Encoding NON_BOM_UTF8_ENCORDING = new UTF8Encoding(false);
}
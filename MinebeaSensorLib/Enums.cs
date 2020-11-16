using System;

namespace MinebeaSensorLib
{
    public enum CommandID: byte
    {
        START = 0xf0,
        DATA = 0xE0,
        BOOT = 0xb0,
        STOP = 0xb2,
        RESET = 0xb4,
        STATUS = 0x80,
        SELECT = 0xa0,
        VERSION = 0xA2
    }

    public enum StatusCode: UInt16
    {
        OK = 0x0000,
        Busy = 0x0001,
        NotSupportCommand = 0x8000,
        IllegalCommandFormat = 0x8001,
        IllegalCommandParameter = 0x8002
    }

    public enum StateID: byte
    {
        INITIAL_State = 0x00,
        READY_State = 0x01,
        BOOT_State = 0x02,
        STOP_State = 0x03,
        MEASURE_State = 0x04,
        RESET_State = 0x05,
        ERROR_State = 0xff,
    }

    [Flags]
    public enum SensorUseFlg: byte
    {
        Sensor1IsEnabled = 1,
        Sensor2IsEnabled = 1 << 1,
        Sensor3IsEnabled = 1 << 2,
        Sensor4IsEnabled = 1 << 3,
        Sensor5IsEnabled = 1 << 4,
    }

     [Flags]
    public enum MeasureStatusFlgs: UInt16
    {
        Sensor1Enabled          = 1 << 0,
        Sensor2Enabled          = 1 << 1,
        Sensor3Enabled          = 1 << 2,
        Sensor4Enabled          = 1 << 3,
        Sensor5Enabled          = 1 << 4,
        IsSPI                   = 1 << 5,
        Reserved1               = 1 << 6,
        Reserved2               = 1 << 7,
        SensorComunicateError   = 1 << 8,
        SensorBootError         = 1 << 9,
        SensorMeasureError      = 1 << 10,
        MatrixCalcError         = 1 << 11,
        MeasureCountOverflow    = 1 << 12,
        MeasureTimeOverflow     = 1 << 13,
        Reserved3               = 1 << 14,
        FatalError              = 1 << 15,
    }
}

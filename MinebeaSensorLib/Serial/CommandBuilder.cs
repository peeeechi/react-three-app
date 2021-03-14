using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace MinebeaSensorLib.Serial
{
    public static class CommandBuilder
    {
        public const byte INSTRUCTION_CODE = 0x54;
        public const byte INSTRUCTION_CODE_FOR_IDLE = 0x53;
        public const byte SPI_WRITE_COMMAND = 0x57;
        public const byte IDLE_COMMAND = 0x94;
        public const byte BOARD_ID = 0x00;
        public const byte OPTION_COMMAND = 0x00;
        public const int BAUDRATE = 1000000;
        public const int DATABITS = 8;

        /*
         
                PortName    = comName,
                BaudRate    = 1000000,
                Parity      = Parity.None,
                DataBits    = 8,
                StopBits    = StopBits.One,
                NewLine     = "\r",
         
         */


        public static readonly byte[] BootSelect_Command = new byte[]
        {
            INSTRUCTION_CODE,
            0x02,
            (byte)CommandID.BoardSelect,
            BOARD_ID,
        };

        public static byte[] PowerSwitch_Command(VDD vDD, bool isPowerOn)
        {
            return new byte[]
            {
                INSTRUCTION_CODE,
                // 84,
                0x03,
                (byte)CommandID.PowerSwitch,
                (byte)vDD,
                Convert.ToByte(isPowerOn)
            };
        }

        public static readonly byte[] FirmwareVersion_Command = new byte[]
        {
            INSTRUCTION_CODE,
            0x01,
            (byte)CommandID.FirmwareVersion,
        };

        public static byte[] AxisSelect_Command(AxisID id)
        {
            return new byte[]
            {
                INSTRUCTION_CODE,
                0x02,
                (byte)CommandID.AxisSelect,
                (byte)id
            };
        }

        public static readonly byte[] Idle_Command = new byte[]
        {
            INSTRUCTION_CODE_FOR_IDLE,
            0x02,
            SPI_WRITE_COMMAND,
            // 0x52,
            IDLE_COMMAND,
        };

        public static readonly byte[] Bootload_Command = new byte[]
        {
            INSTRUCTION_CODE,
            0x01,
            (byte)CommandID.Bootload,
            //0x00
        };

        public static byte[] Coefficient_Command(AxisID axisId, CoefficientID coefficientID)
        {
            return new byte[]
            {
                INSTRUCTION_CODE,
                0x03,
                (byte)CommandID.Coefficient,
                (byte)axisId,
                (byte)coefficientID,
            };
        }

        public static byte[] IntervalMeasure_Command(UInt32 intervalMicroSec = 1000)
        {
            if (intervalMicroSec > 10000000) throw new ArgumentOutOfRangeException();

            var bytes = BitConverter.GetBytes(intervalMicroSec);
            return new byte[]
            {
                INSTRUCTION_CODE,
                0x04,
                (byte)CommandID.IntervalMeasure,
                //bytes[0],
                //bytes[1],
                //bytes[2],

                bytes[2],
                bytes[1],
                bytes[0],
            };
        }

        public static byte[] IntervalRestart_Command(UInt32 intervalMicroSec = 1000)
        {
            if (intervalMicroSec > 10000000) throw new ArgumentOutOfRangeException();

            var bytes = BitConverter.GetBytes(intervalMicroSec);
            return new byte[]
            {
                INSTRUCTION_CODE,
                0x04,
                (byte)CommandID.IntervalRestart,
                bytes[2],
                bytes[1],
                bytes[0],
            };
        }

        public static readonly byte[] Start_Command = new byte[]
        {
            INSTRUCTION_CODE,
            0x02,
            (byte)CommandID.Start,
            OPTION_COMMAND
        };

        public static readonly byte[] Stop_Command = new byte[]
        {
            INSTRUCTION_CODE,
            0x01,
            (byte)CommandID.Stop
        };
    }
}


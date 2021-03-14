using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace MinebeaSensorLib.Serial
{
    /// <summary>
    /// Serial version のコマンドID一覧
    /// </summary>
    public enum CommandID: byte
    {

        /// <summary>
        /// コントローラを選択するコマンド。
        /// コントローラの
        /// Board ID を選択することで接続を確立します。
        /// コントローラとの通信をする前に必ず本コマンドを実行してください。
        /// </summary>
        BoardSelect = 0x10,

        /// <summary>
        /// コントローラのファームウェアバージョンを読み出すコマンド
        /// </summary>
        FirmwareVersion = 0x15,

        /// <summary>
        /// コントローラ上の電源（
        /// LDO ）の ON/OFF を切り替えるコマンド。
        /// </summary>
        PowerSwitch = 0x36,

        /// <summary>
        /// 力覚センサの各軸に対応する
        /// Slave Address を設定するコマンド。
        /// （I2C 版のみ）
        /// </summary>
        SlaveAddress = 0x1B,

        /// <summary>
        /// アクセスする軸を選択するコマンド（
        /// SPI 版のみ）
        /// </summary>
        AxisSelect = 0x1C,

        /// <summary>
        /// 力覚センサを
        /// Idle 状態にするコマンド
        /// 本コマンドのみ、他のコマンドとフォーマットが異なります。
        /// </summary>
        Idle =0x94,

        /// <summary>
        /// コントローラが力覚センサからマトリクス係数を読み出すコマンド。
        /// </summary>
        Bootload = 0xB0,

        /// <summary>
        /// コントローラに保持しているマトリクス係数をホストに転送するコマンド。
        /// </summary>
        Coefficient = 0x27,

        /// <summary>
        /// データの取得間隔を設定するコマンド。
        /// </summary>
        IntervalMeasure = 0x43,

        /// <summary>
        /// 温度更新間隔を設定するコマンド。
        /// </summary>
        IntervalRestart = 0x44,

        /// <summary>
        /// 測定開始コマンド。
        /// </summary>
        Start = 0x23,

        /// <summary>
        /// 測定終了コマンド。
        /// </summary>
        Stop = 0x33,
    }

    /// <summary>
    /// Serial版の応答ID一覧
    /// </summary>
    public enum ResponseStatus: byte
    {
        /// <summary>
        /// エラーなし
        /// </summary>
        OK = 0x00,

        /// <summary>
        /// Command 不正なコマンド
        /// （不正なタイミング時に応答します）
        /// </summary>
        IllegalCommand = 0x01,

        /// <summary>
        /// 不正なパラメータ
        /// 不正なパラメータ時に応答します）
        /// </summary>
        IllegalCommandParameter = 0x03,

        /// <summary>
        /// 力覚センサへのアクセスに失敗
        /// </summary>
        SensorAccessError = 0x08,

        /// <summary>
        /// 非サポートコマンド
        /// (不正なコマンドID 時に応答します)
        /// </summary>
        NotSupportCommand = 0x10,

        Unknown = 0xff,

    }

    public enum VDD: byte
    {
        VDD12 = 0x00,
        VDD33 = 0x01,
        VDD58 = 0x02,
        VDD65 = 0x03,
        VDD45 = 0x05,
    }

    public enum AxisID:byte
    {
        Fx= 0x00,
        Fy= 0x01,
        Fz= 0x02,
        Mx= 0x03,
        My= 0x04,
        Mz= 0x05,
    }

    public enum CoefficientID: byte
    {
        Coefficient1 = 0x00,
        Coefficient2 = 0x01,
        Coefficient3 = 0x02,
        Coefficient4 = 0x03,
        Coefficient5 = 0x04,
        Coefficient6 = 0x05,
    }
}


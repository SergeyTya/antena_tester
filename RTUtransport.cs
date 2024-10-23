using CanGPSLogger;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace antena_tester
{
    internal class RTUtransport: IEnableLogger
    {

        public enum SlaveState {
            onFault,
            onRdy,
            onRun
        }

        SerialPort port = new SerialPort();
        private bool pak_aqn = false;

        public bool connected = false;
        public bool receiveTimeout = false;
        public long mesuaredTimeStepMs = 0;
        public double real_time = 0;
        public double speed_now = 0;

        
        public SlaveState slaveState = SlaveState.onFault; 
        private int byte_requsted = 0;
        static readonly Stopwatch timer_wdg = new Stopwatch();
        static readonly Stopwatch timer_cycle = new Stopwatch();
        static readonly Stopwatch timer_cycle2 = new Stopwatch();
        IFileWriter fw;

        private IObservable<long> wdg = Observable.Interval(TimeSpan.FromSeconds(0.5));

        public RTUtransport() {
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataReceived += Port_DataReceived;
            port.ErrorReceived += Port_ErrorReceived;

            real_time = 0;

            wdg.Subscribe((_) =>
            {
                if (timer_wdg.IsRunning)
                {
                    if (timer_wdg.ElapsedMilliseconds > 300)
                    {
                        this.Log().Fatal($"Serial timeout");
                        timer_wdg.Reset();
                        receiveTimeout = true;
                    }
                }
            });
        }

        public void Connect(string name, int speed) {
            real_time = 0;
            port.PortName = name;
            port.BaudRate = speed;
            receiveTimeout = false;
            port.WriteTimeout = 100;
            try { 
                port.Open();
                
                this.Log().Info($"Port {name}:{speed} opened");
                List<byte> req = new List<byte>() { 1, 43, 14, 0x1, 0x1, 177, 183 };

                fw = new FileWriter();
                // serial_write(req, 0x44);
                port.ReadExisting();
                port.Write(req.ToArray(), 0, req.Count);

            }
            catch(Exception e) { 
                this.Log().Fatal(e);
            }
        }

        private void serial_write(List<byte> req, int byteToRead) {

            this.Log().Info("rcv");
            UInt16 crc = chMBCRC16(req.ToArray(), (ushort)req.Count);

            req.Add((byte)(0x00FF & crc));
            req.Add((byte)((0xFF00 & crc) >> 8));
            byte_requsted = byteToRead;
            if (timer_wdg.IsRunning)
            {
                this.Log().Error($"Skip write request. Port is on wait");
                return;
            }
            port.ReadExisting();
            port.Write(req.ToArray(), 0, req.Count);
            timer_wdg.Restart();
        }

        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            var size = port.BytesToRead;
            if (size < byte_requsted && !receiveTimeout) {

                return;
            };

            timer_wdg.Stop();

            if (size < 3) return;

            byte[] data = new byte[size];
            port.Read(data, 0, size);

            UInt16 crc = chMBCRC16(data, (ushort) (size - 2));
            UInt16 frame_crc = (UInt16)(data[size-2] + (data[size - 1] << 8));

            if (crc == frame_crc)
            {

                // ID
                if (data[1] == 0x2b)
                {

                    string mes = System.Text.Encoding.UTF8.GetString(data);
                    string strDevID = Regex.Replace(mes.Substring(10), @"[^0-9a-zA-Z-_. ]+", " ");
                    this.Log().Info($"Device ID [{strDevID}]");
                    connected = true;
                }
                else if (data[1] == 0x17)
                {
                    //this.Log().Info($"0x17");

                }
                else if (data[1] == 0x6)
                {
                    List<byte> req = new List<byte>() {
                        1, 0x4,
                        0x00, 0x02,
                        0x00, 0x09
                    };
                    serial_write(req, 18);

                } else if (data[1] == 0x4)
                {
                    timer_cycle2.Stop();
                   // Debug.WriteLine(timer_cycle2.ElapsedMilliseconds);
                    switch (data[3]) {
                        case 0x4:
                        case 0x1: // FAULT
                            slaveState = SlaveState.onFault;
                            break;
                        case 0x2: // READY
                            slaveState = SlaveState.onRdy;
                            break;
                        case 0x3: // RUN
                            slaveState = SlaveState.onRun;
                            break;
                    }
                    var pld = new byte[data.Length - 3];
                    Array.Copy(data, 3, pld, 0, pld.Length);
                    var pld16 = ConvertFromByte(pld);
                    fw.writeLine(real_time, pld16);
                    speed_now = pld16[1];

                }
                else {

                    this.Log().Info($"Unknow responce 0x{data[1].ToString("X2")}");
                }


            }
            else {
                receiveTimeout = true;
                port.ReadExisting();
                this.Log().Fatal($"Receive CRC error");
            }
        }

        public void Disconnect() {
            port.Close();
            connected = false;
            receiveTimeout = false;
            timer_cycle2.Reset();
            timer_wdg.Reset();
            timer_cycle.Stop();
            speed_now = 0;
        }

        int tmp_cntr = 0;
        public void SendSpeed(UInt16 speed) {
            if (!port.IsOpen || !connected) { 
                connected = false;
                return;
            }


            real_time += (double)(mesuaredTimeStepMs) / 1000.0;

            //List<byte> req = new List<byte>() {
            //            1, 0x17,
            //            0x00, 0x00, 0x00, 0x01,
            //            0x00, 0x00, 0x00, 0x01,
            //            0x02,
            //            0x00, 0x04
            //};

            //serial_write(req, 2+5);



            mesuaredTimeStepMs = timer_cycle.ElapsedMilliseconds;
            timer_cycle.Restart();

            switch (slaveState)
            {
                case SlaveState.onFault:
                    List<byte> req1 = new List<byte>() {
                        1, 0x6,
                        0x00, 0x00,
                        0x00, 0x04
                    };
                    serial_write(req1, 6);

                    break;
                case SlaveState.onRdy:
                    List<byte> req2 = new List<byte>() {
                        1, 0x6,
                        0x00, 0x00,
                        0x00, 0x01
                    };
                    serial_write(req2, 6);
                    break;
                case SlaveState.onRun:
                    List<byte> req3 = new List<byte>() {
                        1, 0x6,
                        0x00, 0x03,
                        (byte) ((speed&0xFF00)>>8), (byte) (speed&0x00FF)
                    };

                    serial_write(req3, 6);
                break;
            }

            timer_cycle2.Restart();

        }


        static public UInt16 chMBCRC16(byte[] pucFrame, UInt16 len)
        {
            byte ucCRCHi = 0xFF;
            byte ucCRCLo = 0xFF;
            int iIndex;

            for (int i = 0; i < len; i++)
            {
                try
                {
                    iIndex = ucCRCLo ^ pucFrame[i];
                    ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                    ucCRCHi = aucCRCLo[iIndex];
                }
                catch (Exception er) { return 0; };
            }

            return (UInt16)(ucCRCLo + (ucCRCHi << 8));
        }

        static public UInt16[] ConvertFromByte(byte[] data)
        {
            int index = 0;
            var res = data.GroupBy(x => (index++) / 2).Select(x => BitConverter.ToUInt16(x.Reverse().ToArray(), 0)).ToList();
            return res.ToArray();
        }


        static byte[] aucCRCLo = {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
        0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
        0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
        0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
        0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
        0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
        0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
        0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
        0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
        0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
        0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
        0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
        0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
        0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
        0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
        0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
        0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
        0x41, 0x81, 0x80, 0x40};
        static byte[] aucCRCHi = {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40};
    }
}


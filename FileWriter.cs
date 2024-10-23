
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CanGPSLogger
{
    internal class FileWriter: IEnableLogger, IFileWriter
    {
        private StreamWriter? writer;
        string path = @"./DataLogger/";
        string fileName = String.Empty;
        string timestamp = String.Empty;
        static string gps_line_now = String.Empty;
        int counter = 0;

        public FileWriter()
        {
            timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            path = @$"./DataLogger/{timestamp}/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                NewFile();
            }
            this.Log().Info($"New DataLogger started {System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/DataLogger/{timestamp}/");
        }

        public void writeLine(double time, UInt16[] data)
        {

            var str_time = time.ToString("0.000");
           string line = $"{str_time};";

           for (int i = 0; i < data.Length; i++)
           {
                line += $"{data[i].ToString()};";
           }

            writer?.WriteLine(line);
        }


        private void NewFile()
        {
            try
            {
                writer?.Flush();
                writer?.Close();
            }
            catch (System.ObjectDisposedException ex)
            {
                this.Log().Fatal(ex.Message);
            }


            if (File.Exists(path + fileName))
            {
                string new_name = fileName.Substring(0, fileName.Length - 1);
                File.Move(path + fileName, path + new_name);
                this.Log().Info($"Log file created {new_name}");

            }
            fileName = $"{timestamp}_DataLogger_{counter}.csv";
            writer = new(path + fileName);
            counter++;

        }


    }
}

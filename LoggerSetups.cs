using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Splat;

namespace CanGPSLogger
{
    public class LoggerSetups: IEnableLogger
    {
        [JsonProperty("ComName")]
        public string comName { get; set; }

        [JsonProperty("ComSpeed")]
        public int comSpeed { get; set; }

        internal LoggerSetups() {
            comName = "com1";
            comSpeed = 4800;
        }

        public void write()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            File.WriteAllText("logger_setups.json", jsonString);
            this.Log().Error("New setting file created");
        }

        public static LoggerSetups read()
        {
            try
            {
                string jsonString = File.ReadAllText("logger_setups.json", Encoding.Default);

                var inst = JsonConvert.DeserializeObject<LoggerSetups>(jsonString);
                inst?.Log().Info($"Got settings from file [{inst.comName}],[{inst.comSpeed}]");
                return inst;
            }
            catch (Exception e)
            {
                LoggerSetups inst = new LoggerSetups();
                inst.write();
                
                return inst;
            }
        }

    }
}

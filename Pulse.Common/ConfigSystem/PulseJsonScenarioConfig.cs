using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Pulse.Common.ConfigSystem
{
    public class PulseJsonScenarioConfig : PulseScenarioConfig
    {
        public JObject RawJsonConfig { get; set; }

        public PulseJsonScenarioConfig(string fileName)
        {
            RawJsonConfig = GetJObject(fileName);
        }

        private JObject GetJObject(string configPath)
        {
            if (!File.Exists(configPath)) throw new ArgumentException($"Scenario config not found: {configPath}");
            var jsnClasses = JObject.Parse(File.ReadAllText(configPath));

            return jsnClasses;
        }
    }
}

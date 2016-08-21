using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Scenery.Loaders;
using Pulse.Social.Population;

namespace Pulse.Social.Data
{
    public class PhysicalCapabilityClassReader : AbstractFileDataReader
    {
        public IList<IPhysicalCapabilityClass> Classes { get; private set; }

        public PhysicalCapabilityClassReader(string direPath) : base(direPath)
        {
            DataFile = @"population\classes_behavior.json";
            Name = "Physical capabilities class reader";
        }

        protected override void LoadData()
        {
            //var path = @"C:\work\data\input_data\VO\population\classes_behavior.json";
            var jsnClasses = JArray.Parse(File.ReadAllText(DataFullPath));

            var classes = jsnClasses.Select(c => new PhysicalCapabilityClass
                {
                    Id = c["id"].Value<int>(),
                    Name = c["name"].Value<string>(),
                    Agility = ParseInterval(c["agility"].Value<string>()),
                    Stamina = ParseInterval(c["stamina"].Value<string>()),
                    Control = ParseInterval(c["control"].Value<string>()),
                    Information = ParseInterval(c["information"].Value<string>()),
                    Role = ParseInterval(c["role"].Value<string>()),
                    Strength = ParseInterval(c["strength"].Value<string>()),
                    Passability = ParseInterval(c["passability"].Value<string>()),
                    Marital = ParseInterval(c["marital"].Value<string>()),
                    Speed = c["speed"]["walk"].Value<double>()
                    
                }).ToList<IPhysicalCapabilityClass>();

            var cl = classes.Count;
            Classes = classes;
        }

        private IList<int> ParseInterval(string strInterval)
        {
            return strInterval.Split('/').Select(p => Int32.Parse(p)).ToList();
        } 
    }
}

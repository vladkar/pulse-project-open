using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Scenery.Loaders;
using Pulse.Social.Data;

namespace Pulse.Social.Population.General
{
    public class ClassMappingReader : AbstractFileDataReader
    {
        public IDictionary<ISocialEconomyClass, int> ClassRaiting { get; private set; }
        public IDictionary<ISocialEconomyClass, IDictionary<IPhysicalCapabilityClass, int>> MappingTable { get; private set; }

        private readonly PhysicalCapabilityClassReader _pccr;
        private readonly SocialEconomyClassReader _ser;

        public ClassMappingReader(string direPath, PhysicalCapabilityClassReader pccr, SocialEconomyClassReader ser): base(direPath)
        {
            _pccr = pccr;
            _ser = ser;
            DataFile = @"population\society.csv";
            Name = "Agent classes mapper";
        }

        protected override void LoadData()
        {
            if (!_pccr.IsLoaded)
                _pccr.Initialize();
            if(!_ser.IsLoaded)
                _ser.Initialize();
            //var path = @"C:\work\data\input_data\VO\population\society.csv";

            const char csvSeparator = ';';
            var mappingTableStr =
                File.ReadAllLines(DataFullPath)
                    .Select(row => row.Split(csvSeparator).Select(el => el.Trim()).ToArray())
                    .ToArray();
            var clsMappingFSM = new Dictionary<ISocialEconomyClass, IDictionary<IPhysicalCapabilityClass, int>>();
            var clsRaitingFSM = new Dictionary<ISocialEconomyClass, int>();

            for (var iCol = 1; iCol < mappingTableStr.Length; iCol++)
            {
                SocialEconomyClass secl;
                try
                {
                    secl = _ser.Classes.First(cl => cl.Name == mappingTableStr[iCol][0]);
                }
                catch (Exception e)
                {
                    throw new Exception("Population: Social economy class error in " + mappingTableStr[iCol][0] + ", " + e.Message);
                }

                //row
                clsMappingFSM[secl] = new Dictionary<IPhysicalCapabilityClass, int>();

                clsRaitingFSM[secl] = Int32.Parse(mappingTableStr[iCol][1]);

                for (var iRow = 2; iRow < mappingTableStr[0].Length; iRow++)
                {
                    IPhysicalCapabilityClass phcl;
                    try
                    {
                        phcl = _pccr.Classes.First(cl => cl.Name == mappingTableStr[0][iRow]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Population: Physical capability class error in " + secl + " : " + mappingTableStr[0][iRow]);
                    }

                    clsMappingFSM[secl][phcl] = Int32.Parse(mappingTableStr[iCol][iRow]);
                }
            }

            MappingTable = clsMappingFSM;
            ClassRaiting = clsRaitingFSM;
        }
    }
}

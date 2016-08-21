using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pulse.Common;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;

namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class InfectionDataReader : AbstractFileDataReader
    {
        public Dictionary<BaseInfectionStage.InfectionStates, IList<Tuple<BaseInfectionStage.InfectionStates, double>>> InfectionStateChangeProbabilities { private set; get; }
        public Dictionary<BaseInfectionStage.InfectionStates, Tuple<TimeSpan, TimeSpan>> InfectionStatesConstraints { private set; get; }
        public Dictionary<BaseInfectionStage.InfectionStates, string> InfectionStates { private set; get; }
        public InfectionInfo InfectionInfo { private set; get; }

        private string _infection;

        public InfectionDataReader(string direPath, string infection) : base(direPath)
        {
            _infection = infection;
            Name = "Infection reader";
        }

        protected override void LoadData()
        {
            LoadInfectionInfo();
            LoadStateChangeProbabilities();
            LoadStateChangeConstraints();
        }

        private void LoadInfectionInfo()
        {
            var jsnInfection = JObject.Parse(File.ReadAllText(String.Format(@"{0}infection\{1}\infection.json", DataDirectory, _infection)));

            var infectionInfo = new InfectionInfo
            {
                Name = jsnInfection["name"].Value<string>(),
                MatrixPhaseUpdatePeriodMinutes = jsnInfection["matrix_phase_update_period"].Value<int>(),
                InfectionTransmissionTypes = new Dictionary<string, InfectionTransmission>()
            };

            var transmissions = jsnInfection["transmission"].ToObject<string[]>();

            if (transmissions.Contains("droplet"))
            {
                infectionInfo.InfectionTransmissionTypes["droplet"] = new DropletInfectionTransmission
                {
                    NoSymptomsRadius = jsnInfection["droplet"]["no_symptoms_radius"].Value<int>(),
                    SymptomsRadius = jsnInfection["droplet"]["symptoms_radius"].Value<int>(),
                    ContactUpdatePeriodMinutes = jsnInfection["droplet"]["contact_update_period"].Value<int>(),
                    PoissonMeanMinutes = jsnInfection["droplet"]["poisson_mean"].Value<int>()
                };
            }

            if (transmissions.Contains("direct_contact"))
            {
                infectionInfo.InfectionTransmissionTypes["direct_contact"] = new DirectContactInfectionTransmission
                {
                    ActivePeriodMinutes = jsnInfection["direct_contact"]["active_period"].Value<int>()
                };
            }

            InfectionInfo = infectionInfo;
        }

        private void LoadStateChangeConstraints()
        {
            var lines =
                File.ReadAllLines(String.Format(@"{0}\infection\{1}\infection_bernulli_tail_constrains.csv", DataDirectory, _infection))
                    .Skip(1)
                    .Select(l => l.Split(';').Select(el => el.Trim()).ToArray())
                    .ToArray();

            InfectionStatesConstraints = lines.ToDictionary(l => GetStateByName(l[1]), l => new Tuple<TimeSpan, TimeSpan>(ParseTimeSpan(l[0]), ParseTimeSpan(l[2])));
        }

        private TimeSpan ParseTimeSpan(string src)
        {
            var splittedComponents = src.Split(':');
            return new TimeSpan(Int32.Parse(splittedComponents[0]), Int32.Parse(splittedComponents[1]), Int32.Parse(splittedComponents[2]), 0);
        }

        private void LoadStateChangeProbabilities()
        {
            var lines =
                File.ReadAllLines(String.Format(@"{0}\infection\{1}\infection_matrix.csv", DataDirectory, _infection))
                    .Skip(0)
                    .Select(l => l.Split(';').Select(el => el.Trim()).ToArray())
                    .ToArray();
            InfectionStateChangeProbabilities =
                new Dictionary<BaseInfectionStage.InfectionStates, IList<Tuple<BaseInfectionStage.InfectionStates, double>>>();
            InfectionStates = new Dictionary<BaseInfectionStage.InfectionStates, string>();

            for (var i = 1; i < lines.Length; i++)
            {
                InfectionStateChangeProbabilities.Add(GetStateByName(lines[i][0]),
                    new List<Tuple<BaseInfectionStage.InfectionStates, double>>());

                for (var j = 1; j < lines[0].Length - 1; j++)
                {
                    double probability;
                    if (!Double.TryParse(lines[i][j], NumberStyles.Any, CultureInfo.InvariantCulture, out probability)) continue;
                    var tuple = new Tuple<BaseInfectionStage.InfectionStates, double>(GetStateByName(lines[0][j]), probability);
                    InfectionStateChangeProbabilities[GetStateByName(lines[i][0])].Add(tuple);
                }

                InfectionStates.Add(GetStateByName(lines[i][0]), lines[i][lines[0].Length - 1]);
            }
        }

        public BaseInfectionStage.InfectionStates GetState(BaseInfectionStage.InfectionStates currentState)
        {
            return InfectionStateChangeProbabilities[currentState].ProportionChoise(el => el.Item2).Item1;
        }

        public BaseInfectionStage.InfectionStates GetStateExclude(BaseInfectionStage.InfectionStates currentState)
        {
            return InfectionStateChangeProbabilities[currentState].Where(el => el.Item1 != currentState).ProportionChoise(el => el.Item2).Item1;
        }

        public BaseInfectionStage.InfectionStates GetStateExcludeSafe(BaseInfectionStage.InfectionStates currentState)
        {
            return InfectionStateChangeProbabilities[currentState].Count > 1
                ? GetStateExclude(currentState)
                : InfectionStateChangeProbabilities[currentState][0].Item1;
        }

        public TimeSpan GetConstraintMin(BaseInfectionStage.InfectionStates currentState)
        {
            return InfectionStatesConstraints[currentState].Item1;
        }

        public TimeSpan GetConstraintMax(BaseInfectionStage.InfectionStates currentState)
        {
            return InfectionStatesConstraints[currentState].Item2;
        }
        
        public BaseInfectionStage.InfectionStates GetStateByName(string stage)
        {
            switch (stage)
            {
                case "IM":
                    return BaseInfectionStage.InfectionStates.IM;
                    break;
                case "S":
                    return BaseInfectionStage.InfectionStates.S;
                    break;
                case "E":
                    return BaseInfectionStage.InfectionStates.E;
                    break;
                case "I1":
                    return BaseInfectionStage.InfectionStates.I1;
                    break;
                case "I2":
                    return BaseInfectionStage.InfectionStates.I2;
                    break;
                case "T":
                    return BaseInfectionStage.InfectionStates.T;
                    break;
                case "R":
                    return BaseInfectionStage.InfectionStates.R;
                    break;
                case "C":
                    return BaseInfectionStage.InfectionStates.C;
                    break;
                default:
                    throw new Exception(String.Format("Infection stage {0} not found", stage));
            }
        }
    }
}

using System;

namespace Pulse.MultiagentEngine.Stats
{
    public class SimulationInfo
    {
//        private Dictionary<string, object> Values = new Dictionary<string, object>();

//        public object this[string key]
//        {
//            get
//            {
//                if (Values.ContainsKey(key))
//                    return Values[key];
//                else
//                    return 0f;
//            }
//            set { Values[key] = value; }
//        }

//        public Dictionary<string, object>.Enumerator GetEnumerator()
//        {
//            return Values.GetEnumerator();
        //        }

        #region WholeSimulation

        public ulong InitialAgentsCount;
        public string WorldName;
        public DateTime StartedAt;
        public DateTime SecondIteratiobStartedAt;
        public float ConfigurationTimeStep;
        public float ConfigurationToSecondsMultiplier;
        public float ConfigurationSimulationEndsAt;
        public float SimTimeStart;
        public string State;
        public DateTime FinishedAt;
        public float SimTimeEnd;
        #endregion

        #region EachIteration
        public ulong StepNumber;
        public float StepExecutionTime;
        public float StepsPerSecond;
        public float SimulationToRealTimeRate;
        public ulong AgentsCount;
        public ulong MigratedTo;
        public double Time;
        public float SyncTime;
        public uint AgentsGenerated;

        #endregion
    }
}

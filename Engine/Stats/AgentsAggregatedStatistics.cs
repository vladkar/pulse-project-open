using System.Collections.Generic;
using System.Linq;

namespace Pulse.MultiagentEngine.Stats
{
    public enum StatResetMode
    {
        ResetEachStep,
        Never
    }

    public enum StatAgregator
    {
        Avg,
        Sum,
        Max,
        Min
    }

    struct TimestampedValue
    {
        public double Value { get; set; }
        public double SimTimestamp { get; set; }
    }

    /// <summary>
    /// Stats container and engine
    /// </summary>
    public class AgentsAggregatedStatistics
    {
        private Dictionary<string, StatisticUnit> Stats = new Dictionary<string, StatisticUnit>();

        public void AddStat(StatisticUnit newStat)
        {
            Stats.Add(newStat.Name, newStat);
        }

        public StatisticUnit this[string key]
        {
            get { return Stats[key]; }
        }

        public void OnStepFinalize()
        {
            var steped = Stats.Where(pair => pair.Value.ResetMode == StatResetMode.ResetEachStep);
            foreach (var pair in steped)
            {
                pair.Value.ResetValue();
            }
        }

        public Dictionary<string, StatisticUnit>.Enumerator GetEnumerator()
        {
            return Stats.GetEnumerator();
        }
    }



}
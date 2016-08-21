using System;
using System.Collections.Generic;
using Pulse.MultiagentEngine.Agents;

namespace Pulse.MultiagentEngine.Stats
{
    /// <summary>
    /// Class for one stat counter
    /// </summary>
    public class StatisticUnit : IAgentsStatistics
    {
        public string Name { get; private set; }

        public StatResetMode ResetMode { get; private set; }
        public StatAgregator Agregator { get; }
        public ulong HistoryWindowSize { get; set; }

        private LinkedList<TimestampedValue> HistoryWindow = new LinkedList<TimestampedValue>();

        public double Value { get; private set; }
        public ulong Count { get; private set; }

        public StatisticUnit(string name, StatResetMode resetMode, StatAgregator agregator, ulong historyWindowSize)
        {
            ResetMode = resetMode;
            Agregator = agregator;
            HistoryWindowSize = historyWindowSize;
            Name = name;

            ResetValue();
        }

        public void AddStat(double val, double now)
        {
            Count++;

            // window
            if (HistoryWindowSize > 0)
            {
                if (HistoryWindow.Count == (int) HistoryWindowSize)
                {
                    HistoryWindow.RemoveFirst();
                }
                HistoryWindow.AddLast(new TimestampedValue {Value = val, SimTimestamp = now});
            }
          
            AggregationFunction(val);
        }

        private void AggregationFunction(double val)
        {
            switch (Agregator)
            {
                case StatAgregator.Avg:
                    Value = (Value * (Count - 1) + val) / Count;
                    break;
                case StatAgregator.Sum:
                    Value += val;
                    break;
                case StatAgregator.Max:
                    Value = Value > val ? Value : val;
                    break;
                case StatAgregator.Min:
                    Value = Value < val ? Value : val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Switch to default
        /// </summary>
        public void ResetValue()
        {
            switch (Agregator)
            {
                case StatAgregator.Avg:
                    Value = 0;
                    break;
                case StatAgregator.Sum:
                    Value = 0;
                    break;
                case StatAgregator.Max:
                    Value = double.MinValue;
                    break;
                case StatAgregator.Min:
                    Value = double.MaxValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Update(IEnumerable<AgentBase> agents, double simTime)
        {
//            foreach (var agent in agents.OfType<SpatialAgentBase>())
//            {
//                AggregationFunction(agent.Velocity.Magnitude());
//            }
        }

        public void Finalization()
        {
            
        }
    }
}
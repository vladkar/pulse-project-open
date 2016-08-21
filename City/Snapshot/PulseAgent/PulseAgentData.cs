using Pulse.Common.Model.Agent;

namespace City.Snapshot.PulseAgent
{
    public struct PulseAgentData : IPulseAgentData
    {
        public int Id { set; get; }
        public double X { set; get; }
        public double Y { set; get; }
        public byte Level { get; set; }
    }
}
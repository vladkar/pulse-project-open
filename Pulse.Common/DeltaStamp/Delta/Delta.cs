using System.Collections.Concurrent;
using Pulse.Common.Model;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.DeltaStamp
{
    public interface IPulseDelta : IIdentifiedItem
    {
        ConcurrentQueue<IPulseAgentDelta> Agents { set; get; }
    }

    public class PulseDelta : IPulseDelta
    {
        public long Id { get; set; }
        public ConcurrentQueue<IPulseAgentDelta> Agents { get; set; }

        public PulseDelta()
        {
            Agents = new ConcurrentQueue<IPulseAgentDelta>();
        }
    }

    public interface IPulseAgentDeltaData : IIdentifiedItem, IPseudo3DObject
    {
    }

    public interface IPulseAgentDelta : IPulseAgentDeltaData
    {
        void ApplyDelta(IPulseAgentDeltaData recepient);
    }

    public abstract class Delta : IIdentifiedItem
    {
        //public object Type { set; get; }
        public long Id { set; get; }
    }

    public class AgentSimpleDelta : Delta, IPulseAgentDelta
    {
        public PulseVector2 Point { get; set; }
        public int Level { get; set; }

        public void ApplyDelta(IPulseAgentDeltaData recepient)
        {
            recepient.Level = Level;
            recepient.Point = Point;
        }
    }
}

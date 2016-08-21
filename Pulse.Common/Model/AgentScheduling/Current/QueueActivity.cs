using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.AgentScheduling.Current
{
    public class QueueActivity : CurrentActivity
    {
        public QueueablePoi Queueable { get; set; }
    }
}
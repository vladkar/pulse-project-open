namespace Pulse.Common.Model.AgentScheduling.Abstract
{
    // level 1: class activity
    public class AbstractAgentActivity
    {
        public string Name { set; get; }
        public string ActivityType { set; get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
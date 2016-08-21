using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;

namespace Pulse.Common.Behavior.Pulse
{
    public class DecisionTreeData
    {
        public PulseWorld World { set; get; }
        public AbstractPulseAgent Agent {set; get; }
        public Random Random { get; set; }
    }
}
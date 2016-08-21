using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Agents
{
    [Serializable]
    public class PulseAgentsData2 : IPulseAgentsData
    {
        public IPulseAgentData2[] Agents { get; set; }
    }


    [Serializable]
    public struct PulseAgentData3 : IPulseAgentData2
    {
        public long Id { get; set; }
        public bool IsAlive { get; set; }
        public PulseVector2 Point { get; set; }
        public GeoCoords GeoPoint { get; set; }
        public int Floor { get; set; }
        public int SocialEconomyClass { get; set; }
        public int PhysicalCapabilityClass { get; set; }
        public bool IsInBuilding { get; set; }
        public byte InfectionState { get; set; }


        public PluginsContainer PluginsContainer { get; set; }
    }



    [Serializable]
    public struct PulseAgentData4 : IPulseAgentData4
    {
        public PluginsContainer PluginsContainer { get; set; }
        public long Id { get; set; }
        public bool IsAlive { get; set; }
        public PulseVector2 Point { get; set; }
        public GeoCoords GeoPoint { get; set; }
        public int Floor { get; set; }
        public int SocialEconomyClass { get; set; }
        public int PhysicalCapabilityClass { get; set; }
        public bool IsInBuilding { get; set; }
        public byte InfectionState { get; set; }
        public Tuple<byte, PulseVector2, PulseVector2, PulseVector2> NavInfo { get; set; }
    }

    [Serializable]
    public class PulseDeadAgentsData : IAgentsData
    {
        public IPulseDeadAgentData[] Agents { get; set; }
    }

    [Serializable]
    public struct PulseDeadAgentData : IPulseDeadAgentData
    {
        public long Id { get; set; }
        public bool IsAlive { get; set; }
        public PulseVector2 Point { get; set; }
        public GeoCoords GeoPoint { get; set; }
        public int Floor { get; set; }
        public int SocialEconomyClass { get; set; }
        public int PhysicalCapabilityClass { get; set; }
        public bool IsInBuilding { get; set; }
        public byte InfectionState { get; set; }
        public int TerminationReason { get; set; }


        public PluginsContainer PluginsContainer { get; set; }
    }
}

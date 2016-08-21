using System;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Interface;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{


    public interface IPulseAgentsData : IAgentsData
    {
        IPulseAgentData2[] Agents { get; set; }
    }

    public interface IPulseAgentData2 : IPluginable
    {
        long Id { set; get; }
        bool IsAlive { get; set; }
        PulseVector2 Point { set; get; }
        GeoCoords GeoPoint { set; get; }
        int Floor { set; get; }
        int SocialEconomyClass { get; set; }
        int PhysicalCapabilityClass { get; set; }
        bool IsInBuilding { set; get; }
        byte InfectionState { set; get; }
    }

    public interface IPulseAgentData4 : IPulseAgentData2
    {
        Tuple<byte, PulseVector2, PulseVector2, PulseVector2> NavInfo { set; get; }
    }

    public interface IPulseDeadAgentData : IPulseAgentData2
    {
        int TerminationReason { set; get; }
    }


    //todo int instead of long for net transport optimization
    public interface IPulseAgentData
    {
        int Id { set; get; }    
        double X { set; get; }
        double Y { set; get; }
        byte Level { set; get; }
    }

    public interface IGuestAgent : IPulseAgentData
    {
        byte HomeWorld { set; get; }
        byte DestWorld { set; get; }
        int Portal { set; get; }
    }

    public interface ISfAgent : IPulseAgentData
    {
        double Pressure { set; get; }
        double StepDist { set; get; }
        short Angle { get; set; }
        double ForceX { get; set; }
        double ForceY { get; set; }
        byte Role { get; set; }
    }


    public class SimpleGuestAgent : IGuestAgent
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public byte Level { get; set; }
        public byte HomeWorld { get; set; }
        public byte DestWorld { get; set; }
        public int Portal { get; set; }
    }
}
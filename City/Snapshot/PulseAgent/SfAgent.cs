using System;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Pulse.Common.Model.Agent;

namespace City.Snapshot.PulseAgent
{
    public struct SfAgent : ISfAgent
    {
        public double X { get; set; }
        public double Y { get; set; }
        public byte Level { get; set; }
        public int Id { get; set; }
        public byte Role { get; set; }
        public double Pressure { get; set; }
        public double StepDist { get; set; }
        public short Angle { get; set; }
        public double ForceX { get; set; }
        public double ForceY { get; set; }

        public SfAgent(AbstractPulseAgent agent)
        {
            Id = Convert.ToInt32(agent.Id);
            X = agent.Point.X;
            Y = agent.Point.Y;
            Pressure = agent.Pressure;
            StepDist = agent.StepDistance;
            Angle = (short) DMathUtil.RadiansToDegrees(Math.Atan2(agent.DesiredPosition.Y - agent.Point.Y, agent.DesiredPosition.X - agent.Point.X));
            Level = (byte) agent.Level;
            ForceX = agent.Force.X;
            ForceY = agent.Force.Y;
            Role = (byte) agent.Role.Id;
        }
    }
}
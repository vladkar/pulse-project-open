using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.Poi
{
    public class ExternalPortal : IExternalPortal
    {
        public PulseVector2 Point { set; get; }
        public PulseVector2 TravelPoint { get { return  NavgationBlock.GetNavigationPoint(); } }
        public PulseVector2[] Polygon { get; set; }
        public IList<PointOfInterestType> Types { get; set; }
        public IPointOfInterestNavgationBlock NavgationBlock { get; set; }
        public PluginsContainer PluginsContainer { get; set; }
        public int PoepleAmount { get; set; }
        public IList<AbstractPulseAgent> Agents { get; set; }
        public event OnAgentExitDelegate OnAgentExit;
        public event OnAgentEnterDelegate OnAgentEnter;

        public ExternalPortal()
        {
            Agents = new List<AbstractPulseAgent>();
            Types = new List<PointOfInterestType>();;
        }

        public ExternalPortal(ILevelPortal p) : this()
        {
            ObjectId = p.ObjectId;
            Name = p.Name;
            Point = p.Point;
            Polygon = p.Polygon;
            Level = p.Level;
            Enterable = p.Enterable;
            Exitable = p.Exitable;
            Zone = p.Zone;
            NavgationBlock = p.NavgationBlock;
        }

        public PulseVector2 GetTravelPoint(AbstractPulseAgent agen, Random r)
        {
            return ClipperUtil.GetRandomPointOnPolygon(Polygon);
        }

        public virtual void Enter(AbstractPulseAgent agent)
        {
            OnAgentEnterInvoke(agent);
            agent.Kill("migration_" + ((Types != null && Types.Any()) ? Types.First().Name : "null"));
        }

        public void Exit(AbstractPulseAgent agent)
        {
            OnAgentExitInvoke(agent);
        }

        protected void OnAgentExitInvoke(AbstractPulseAgent agent)
        {
            var handler = OnAgentExit;
            if (handler != null) handler(agent);
        }

        protected void OnAgentEnterInvoke(AbstractPulseAgent agent)
        {
            var handler = OnAgentEnter;
            if (handler != null) handler(agent);
        }

        public string Name { get; set; }
        public string ObjectId { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }

        public override string ToString()
        {
            return String.Format("Types: {0}, ID: {1}, floor: {2}, point: {3}", TypesToString(), ObjectId, Level, TravelPoint);
        }

        private string TypesToString()
        {
            return String.Join(", ", Types.Select(t => t.Name));
        }

        public virtual void Update(double timeStep, double time, DateTime geotime)
        {
            if (AgentGenerator != null)
                AgentGenerator.Update(timeStep, time, geotime);
        }

        public AbstractExternalPortalAgentGenerator AgentGenerator { get; set; }
        public bool Enterable { get; set; }
        public bool Exitable { get; set; }
        public Zone Zone { get; set; }
    }
}

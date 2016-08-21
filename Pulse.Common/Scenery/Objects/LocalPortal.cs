using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Objects
{
    public class LocalPortal : ILevelPortal, IComplexUpdatable
    {
        public string TargetLink { set; get; }
        public LocalPortal Target { set; get; }
        public bool Enterable { get; set; }
        public bool Exitable { get; set; }

        public PluginsContainer PluginsContainer { get; set; }
        public PulseVector2 TravelPoint { get { return GetEntrance(); } }
        public PulseVector2 Point { get; set; }
        public PulseVector2[] Polygon { get; set; }
        public IList<PointOfInterestType> Types { get; set; }
        public IPointOfInterestNavgationBlock NavgationBlock { get; set; }

        public PulseVector2 GetTravelPoint(AbstractPulseAgent agen, Random r)
        {
            return ClipperUtil.GetRandomPointOnPolygon(Polygon);
        }

        public int PoepleAmount { get; set; }
        public IList<AbstractPulseAgent> Agents { get; set; }
        public event OnAgentExitDelegate OnAgentExit;
        public event OnAgentEnterDelegate OnAgentEnter;

        public LocalPortal()
        {
            Agents = new List<AbstractPulseAgent>();
        }

        private PulseVector2 GetEntrance()
        {
            return NavgationBlock.GetNavigationPoint();
        }

        public void Enter(AbstractPulseAgent agent)
        {
            OnAgentEnterInvoke(agent);

            lock (Agents)
            {
                Agents.Add(agent);
            }
        }

        public void Exit(AbstractPulseAgent agent)
        {
            OnAgentExitInvoke(agent);

            lock (Agents)
            {
                Agents.Remove(agent);
            }
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
        public LevelPortalTransporter PortalTransporter { get; set; }
        public int Level { get; set; }
        //public int Level { get; set; }

        public Zone Zone { get; set; }


        public void Update(double timeStep, double time, DateTime geotime)
        {
            var removeList = new List<AbstractPulseAgent>();
            foreach (var agent in Agents)
            {
                var ca = agent.CurrentActivity as SimplePortalActivity;
                if (ca == null)
                    Debug.Assert(true);

                agent.ChangeLevel(ca.Exit.Level);
                removeList.Add(agent);
                agent.DoneActivity();
            }

            foreach (var agent in removeList)
            {
                Exit(agent);
            }
        }
    }
}

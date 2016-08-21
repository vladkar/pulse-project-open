using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.Poi
{
    public abstract class AbstractInteractivePointOfInterest : AbstractPulseObject, IPointOfInterest, IComplexUpdatable, IActivePoi
    {
        #region IPointOfInterest

        public PluginsContainer PluginsContainer { get; set; }
        public PulseVector2 Point { get; set; }
        public int Level { get; set; }
        public PulseVector2 TravelPoint { get { return NavgationBlock.GetNavigationPoint(); } }
        public PulseVector2[] Polygon { get; set; }
        public IList<PointOfInterestType> Types { get; set; }
        public IPointOfInterestNavgationBlock NavgationBlock { get; set; }

        public PulseVector2 GetTravelPoint(AbstractPulseAgent agen, Random r)
        {
            throw new NotImplementedException();
        }

        public Zone Zone { get; set; }

        #endregion

        protected AbstractInteractivePointOfInterest(IPointOfInterest poi) : this()
        {
            Point = poi.Point;
            Level = poi.Level;
            Polygon = poi.Polygon;
            Types = poi.Types;
            NavgationBlock = poi.NavgationBlock;
            Zone = poi.Zone;
        }

        protected AbstractInteractivePointOfInterest()
        {
            Agents = new List<AbstractPulseAgent>();
        }

        public abstract void Update(double timeStep, double time, DateTime geotime);

        //public int PoepleAmount { get; set; }

        #region IEnterable

        public IList<AbstractPulseAgent> Agents { get; set; }

        public event OnAgentExitDelegate OnAgentExit;
        public event OnAgentEnterDelegate OnAgentEnter;

        public virtual void Enter(AbstractPulseAgent agent)
        {
            OnAgentEnterInvoke(agent);

            lock (Agents)
            {
                Agents.Add(agent);
            }
        }

        public virtual void Exit(AbstractPulseAgent agent)
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

        #endregion

        public override string ToString()
        {
            return String.Format("Types: {0}, ID: {1}, floor: {2}, point: {3}", TypesToString(), ObjectId, Level, TravelPoint);
        } 

        private string TypesToString()
        {
            return String.Join(", ", Types.Select(t => t.Name));
        }
    }
}

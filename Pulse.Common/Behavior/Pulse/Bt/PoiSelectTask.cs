using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class PoiSelectTask : AbstractActionBtNode<DecisionTreeData>
    {
        public PoiSelectTask(DecisionTreeData data) : base(data)
        {
        }

        public override BehaviorState Update()
        {
            var plannedIntention = Data.Agent.Role.Data.PlannedIntention as IPoiPlannedIntension;
            if (plannedIntention == null) return BehaviorState.Failed;

            var pois = new List<IPointOfInterest>();

            foreach (var strType in plannedIntention.PoiTypes)
            {
                var type = (Data.World.Map as IPulseMap).Scenery.GetPoiTypeByName(strType);

                var typedPois = (Data.World.Map as IPulseMap).Scenery.TypedPointsOfInterest[type].Where(p => p.NavgationBlock != null);
                foreach (var p in typedPois)
                {
                    if (p.Level == Data.Agent.Level)
                        pois.Add(p);
                }
            }

            if (pois.Count == 0)
            {
                Data.Agent.Role.Data.PlannedIntention = null;
                return BehaviorState.Failed;
            }

            var i = Data.Random.Next(0, pois.Count);

            var currentIntention = new PoiCurrentIntention(plannedIntention);
            currentIntention.Poi = pois[i];
            currentIntention.GoalPoint = currentIntention.Poi.GetTravelPoint(Data.Agent, Data.Random);
            currentIntention.Level = currentIntention.Poi.Level;
            Data.Agent.Role.Data.CurrentIntention = currentIntention;

            return BehaviorState.Success;
        }
    }
}
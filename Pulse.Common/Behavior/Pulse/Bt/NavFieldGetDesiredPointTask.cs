using System;
using System.Diagnostics;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.NavField;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class NavFieldGetDesiredPointTask : AbstractActionBtNode<DecisionTreeData>
    {
        public BehaviorState State { get; set; }

        public NavFieldGetDesiredPointTask(DecisionTreeData data) : base(data)
        {
            State = BehaviorState.Ready;
        }

        public override BehaviorState Update()
        {
            var currentIntention = Data.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention;
            if (currentIntention == null) return BehaviorState.Failed;

            var sp = currentIntention.Path[currentIntention.CurrentSubPath] as NavFieldTravelPath;
            if (sp == null) return BehaviorState.Failed;

            switch (State)
            {
                case BehaviorState.Ready:
                    Data.Agent.SetMovementSystem(null);
                    State = BehaviorState.Running;
                    break;
                case BehaviorState.Success:
                    return BehaviorState.Success;
            }



//            var nfnv = new PoiTreeNavfieldNavigator((Data.World.Map as IPulseMap).Scenery);

            if (Data.Agent.Point.DistanceTo(currentIntention.GoalPoint) > Data.Agent.Role.Config.PoiInteractionRadius)
            {
                var vel = Data.Agent.Navigator.Getvelocity(Data.Agent.Point, currentIntention.GoalPoint, Data.Agent.Level);
//                var vel = nfnv.Getvelocity(Data.Agent.Point, currentIntention.GoalPoint, Data.Agent.Level);

                Data.Agent.DesiredPosition = Data.Agent.Point + vel;

                return State;
            }


//            if (sp.SimplePath.Count > sp.LastDoneCheckpoint + 1)
//            {
//                var next = sp.SimplePath[sp.LastDoneCheckpoint + 1];
//
//                if (Data.Agent.Point.DistanceTo(next) <= Data.Agent.Role.Config.CornerCutOffRadius)
//                    //if (_agent.Point.DistanceTo(next) <= _cornerCutOffRadius)
//                {
//                    if (sp.LastDoneCheckpoint < sp.SimplePath.Count - 1)
//                        sp.LastDoneCheckpoint++;
//                }
//                else
//                {
//                    //                        if (travelActivity.LastDoneCheckpoint > 0 && MovementSystem.QueryVisibility(Point, next, 0.1f))
//                    //                            travelActivity.LastDoneCheckpoint++;
//                }
//                Data.Agent.DesiredPosition = next;
//
//                return State;
//            }

            State = BehaviorState.Success;
            return State;
        }
    }
}
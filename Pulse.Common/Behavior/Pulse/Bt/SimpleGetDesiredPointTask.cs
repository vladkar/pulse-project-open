using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class SimpleGetDesiredPointTask : AbstractActionBtNode<DecisionTreeData>
    {
        public BehaviorState State { get; set; }

        public SimpleGetDesiredPointTask(DecisionTreeData data) : base(data)
        {
            State = BehaviorState.Ready;
        }

        public override BehaviorState Update()
        {
            var currentIntention = Data.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention;
            if (currentIntention == null) return BehaviorState.Failed;

            var sp = currentIntention.Path[currentIntention.CurrentSubPath] as SimpleTravelPath;
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


            if (sp.SimplePath.Count > sp.LastDoneCheckpoint + 1)
            {
                var next = sp.SimplePath[sp.LastDoneCheckpoint + 1];

                if (Data.Agent.Point.DistanceTo(next) <= Data.Agent.Role.Config.CornerCutOffRadius)
                    //if (_agent.Point.DistanceTo(next) <= _cornerCutOffRadius)
                {
                    if (sp.LastDoneCheckpoint < sp.SimplePath.Count - 1)
                        sp.LastDoneCheckpoint++;
                }
                else
                {
                    //                        if (travelActivity.LastDoneCheckpoint > 0 && MovementSystem.QueryVisibility(Point, next, 0.1f))
                    //                            travelActivity.LastDoneCheckpoint++;
                }
                Data.Agent.DesiredPosition = next;
                
                return State;
            }

            State = BehaviorState.Success;
            return State;
        }
    }
}
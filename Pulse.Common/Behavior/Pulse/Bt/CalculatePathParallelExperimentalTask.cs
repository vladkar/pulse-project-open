using System;
using System.Collections.Generic;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class CalculatePathParallelExperimentalTask : AbstractActionBtNode<DecisionTreeData>
    {
        public CalculatePathParallelExperimentalTask(DecisionTreeData data) : base(data)
        {
        }

        private IList<ITravelPath> _cache;
        private PathCalc.CalcTask _task;
        public override BehaviorState Update()
        {
            var currentIntention = Data.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention;

            if (currentIntention == null) return BehaviorState.Failed;

            try
            {
                if (_task == null)
                {
                    var calcPath = new Action(() =>
                    {
                        _cache = Data.Agent.CalculatePath(Data.Agent.Point, Data.Agent.Level, currentIntention.GoalPoint,
                            currentIntention.Level);
                    });

                    _task = PathCalc.PushTask(calcPath);
                    return BehaviorState.Running;
                }
                else
                {
                    if (_task.State == BehaviorState.Success)
                    {
                        currentIntention.Path = _cache;
                    }

                    return _task.State;
                }


                // task: if ok: return true, cache = res

                //if task == finished: marker = true, path = res, return success
            }
            catch (Exception)
            {
                //TODO log
                return BehaviorState.Failed;
            }
        }
    }
}
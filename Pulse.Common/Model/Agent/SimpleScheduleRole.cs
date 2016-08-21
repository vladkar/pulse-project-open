using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common.Model.AgentScheduling;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Model.Agent
{
    public class SimpleScheduleRoleConfig
    {
        public double DeadZoneRadius { set; get; }
        public double PoiInteractionRadius { set; get; }
        public double CornerCutOffRadius { set; get; }
    }


    public abstract class SimpleScheduleRole : AbstractAgentRole
    {
        protected CurrentActivityFactory _currentActiityFactory;

        protected double _deadZoneRadius = 0.3;
        protected double _poiInteractionRadius = 10;
        protected double _cornerCutOffRadius = 5;

        protected SimpleScheduleRole(AbstractPulseAgent agent, SimpleScheduleRoleConfig config, string name, int id)
            : this(agent, name, id)
        {
            _poiInteractionRadius = config.PoiInteractionRadius;
            _cornerCutOffRadius = config.CornerCutOffRadius;
            _deadZoneRadius = config.DeadZoneRadius;
            Config = config;
        }

        protected SimpleScheduleRole(AbstractPulseAgent agent, string name, int id)
            : base(agent, name, id)
        {
            _currentActiityFactory = new CurrentActivityFactory(agent);
        }

        #region schedule

        public override void CheckSchedule()
        {
            if (PlannedDailyAgentSchedule == null)
            {
                PlannedDailyAgentSchedule = GetPlannedSchedule();
            }
        }

        #endregion

        #region activity


        public override void CheckActivity()
        {
            if (_agent.CurrentActivity == null)
            {
                _agent.CurrentActivity = GetActivity();
            }
            else
            {
                // last unfinished child (go down)
                var child = _agent.CurrentActivity.Child;
                var hasUnfinishedChild = false;
                while (child != null)
                {
                    if (_agent.CurrentActivity.Child.State != ActivityState.Finished)
                    {
                        _agent.CurrentActivity = _agent.CurrentActivity.Child;
                        hasUnfinishedChild = true;
                    }
                    child = child.Child;
                }
                if (hasUnfinishedChild) return;

                if (_agent.CurrentActivity.State == ActivityState.Finished)
                {
                    // first unfinished next
                    var next = _agent.CurrentActivity.Next;
                    while (next != null)
                    {
                        if (next.State != ActivityState.Finished)
                        {
                            _agent.CurrentActivity = next;
                            return;
                        }
                        next = next.Next;
                    }

                    // first unfinished parent
                    var parent = _agent.CurrentActivity.Parent;
                    while (parent != null)
                    {
                        if (parent.State != ActivityState.Finished)
                        {
                            _agent.CurrentActivity = parent;
                            return;
                        }
                        parent = parent.Parent;
                    }

                    var prev = _agent.CurrentActivity;
                    _agent.CurrentActivity = GetActivity();
                    prev.Next = _agent.CurrentActivity;
                    _agent.CurrentActivity.Prev = prev;
                }
            }
        }

        public override CurrentActivity GetActivity()
        {
            //config
            var earlyArrivalGandigap = new TimeSpan(0, 15, 0);
            var speed = _agent.PhysicalCapabilityClass.Speed * 1000d / 3600; // meters per second

            // 1. Queue is not empty?
            // 1 + : queue is not empty
            if (PlannedDailyAgentSchedule.PlannedDailySchedule.Count > 0)
            {
                var nextAction = PlannedDailyAgentSchedule.PlannedDailySchedule.First();

                // 2. Already at goal building?
                // 2 + : at goal building
                if (_agent.Point.DistanceTo(nextAction.Poi.TravelPoint) < _poiInteractionRadius)
                {
                    //                    CurrentBuilding = nextAction.Building;
                    var first = PlannedDailyAgentSchedule.PlannedDailySchedule.First();
                    PlannedDailyAgentSchedule.PlannedDailySchedule.Remove(first);

                    CurrentActivity ca;
                    //2.1 Interactable building?
                    //2.1 - : not interactable
                    var interactable = nextAction.Poi as IInteractable;
                    if (interactable == null)
                        ca = _currentActiityFactory.GetSimpleActionActivity(first);
                    //2.1 + : interactable
                    else
                        ca = _currentActiityFactory.GetPassiveActionActivity(first);


                    // is queueable?
                    //TODO ok check
                    var queueable = nextAction.Poi as QueueablePoi;
                    //                    var queueable = true;
                    if (queueable != null)
                    {
                        // agent already done queue?
                        // if no (-)

                        var isQueueDone = _agent.CurrentActivity?.Child is QueueActivity; // not null and queueactivity


                        if (!isQueueDone)
                        {
                            var qa = new QueueActivity();
                            qa.RealStartTime = _agent.WorldKnowledge.Clock;
                            qa.Queueable = queueable;
                            ca.Child = qa;
                            qa.Parent = ca;

                            //                            return 
                        }
                    }

                    return ca;
                }
                // 2 - : not at goal building
                else
                {
                    var distanceFromCurrentPosToGoal = _agent.Point.DistanceTo(nextAction.Poi.TravelPoint) *
                                                _agent.WorldKnowledge.MetersPerMapUnit; // meters
                    var estimatedTravlTimeFromCurrentPosToGoal = new TimeSpan(0, 0, (int)(distanceFromCurrentPosToGoal / speed));

                    var elapsedTime = nextAction.PlannedStartTime - _agent.WorldKnowledge.Clock;

                    // 3. Should waiting?
                    // 3 + : waiting
                    if (elapsedTime > estimatedTravlTimeFromCurrentPosToGoal + earlyArrivalGandigap)
                    {
                        return _currentActiityFactory.GetWaitingActivity(elapsedTime + earlyArrivalGandigap);
                    }
                    // 3 - : go to next action goal
                    else
                    {
                        return _currentActiityFactory.GetComplexTravelingActivity(nextAction.Poi.TravelPoint, nextAction.Poi.Level);
                    }
                }
            }
            // 1 - : queue is empty 
            else
            {
                return _currentActiityFactory.GetWaitingActivity(new TimeSpan(0, 0, 15));
            }
        }

        public override void DoActivity()
        {
            if (_agent.CurrentActivity is WaitingActivity)
            {
                var ca = _agent.CurrentActivity as WaitingActivity;
                if (ca.PlannedEndTime <= _agent.WorldKnowledge.Clock)
                    _agent.DoneActivity();
            }

            else if (_agent.CurrentActivity is SimpleActionActivity)
            {
                var ca = _agent.CurrentActivity as SimpleActionActivity;
                if (ca.PlannedEndTime <= _agent.WorldKnowledge.Clock)
                    _agent.DoneActivity();
            }

            else if (_agent.CurrentActivity is PassiveActionActivity)
            {
                var ca = _agent.CurrentActivity as PassiveActionActivity;

                // 1. Started?
                // 1.1 - : not started
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(ca.Building);
                    ca.State = ActivityState.Started;
                }

                if (ca.State == ActivityState.Started)
                {
                    //processing by poi
                }
            }

            else if (_agent.CurrentActivity is ComplexTravelingActivity)
            {
                var ca = _agent.CurrentActivity as ComplexTravelingActivity;

                if (ca.State == ActivityState.NotStarted)
                {
                    ca.State = ActivityState.Started;
                }

                // 1. Already at goal?
                // 1 + : at goal
                //if (_agent.Point.DistanceTo(ca.Point) < minTouchDistance & _agent.Floor == ca.Level)
                // 1 + : change finish condition due to container activity
                if (_agent.Point.DistanceTo(ca.Point) < _poiInteractionRadius & _agent.Level == ca.Level || ca.IsChildrenFinish())
                {
                    _agent.CurrentActivity.State = ActivityState.Finished;
                }

                // 1 - : not at goal
                else
                {
                    // 1.2. Is path calculated?
                    // 1.2.-. : not calculated
                    if (ca.Path == null)
                    {
                        // 1.2.1. Is path possible?
                        // 1.2.1.+. : possible, ok
                        try
                        {
                            ca.Path = _agent.CalculatePath(_agent.Point, _agent.Level,
                                ca.Point, ca.Level);
                        }
                        // 1.2.1.-. : impossible, kill agent
                        catch (Exception e)
                        {
                            _agent.Kill("bad_path");
                            ca.Path = new List<ITravelPath>();
                            Log.Error(
                                "Pathfinding trouble. Agent: {0}, Start pos: (point: {1}, level: {2}), Dest pos: (point: {3}, level: {4})",
                                _agent.Id, _agent.Point, _agent.Level, ca.Point,
                                ca.Level);
                            Log.Error("Pathfinding trouble. Exception: {0}, inner exception {1}", e.Message,
                                e.InnerException == null ? "null" : e.InnerException.Message);

//                            throw;
                        }
                    }

                    // 1.2.+. : calculated and existed path: processing complex path
                    if (ca.Path.Count > 0)
                    {
                        var firstSub = _currentActiityFactory.GetTravelingActivityFromTravelingUnit(ca.Path[0]);
                        var prev = firstSub;

                        for (var i = 1; i < ca.Path.Count; i++)
                        {
                            var next = _currentActiityFactory.GetTravelingActivityFromTravelingUnit(ca.Path[i]);

                            prev.Next = next;
                            next.Prev = prev;

                            prev = next;
                        }

                        ca.Child = firstSub;
                        firstSub.Parent = ca;
                    }
                }
            }

            else if (_agent.CurrentActivity is SimplePathTravelingActivity)
            {
                var ca = _agent.CurrentActivity as SimplePathTravelingActivity;

                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.SetMovementSystem(_agent.CurrentActivity as UnitTravelingActivity);
                    ca.State = ActivityState.Started;
                }

                if (_agent.Point.DistanceTo(ca.SimplePath.Last()) < _poiInteractionRadius)
                {
                    _agent.CurrentActivity.State = ActivityState.Finished;
                }

            }

            else if (_agent.CurrentActivity is SimplePortalActivity)
            {
                var ca = _agent.CurrentActivity as SimplePortalActivity;

                // 1. Started?
                // 1.1 - : not started
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(ca.Enter);
                    ca.State = ActivityState.Started;
                }
                else if (ca.State == ActivityState.Started)
                {
                    //processing by poi
                }
            }

            else if (_agent.CurrentActivity is QueueActivity)
            {
                var qa = _agent.CurrentActivity as QueueActivity;

                if (qa.State == ActivityState.NotStarted)
                {
                    qa.Queueable.Enqueue(_agent);
                    qa.State = ActivityState.Started;
                }
            }
        }

        #endregion


        #region moving

        public override void GetDesiredPosition()
        {
           if (_agent.CurrentActivity is SimplePathTravelingActivity)
            {
                var travelActivity = _agent.CurrentActivity as SimplePathTravelingActivity;


                if (travelActivity.SimplePath.Count > travelActivity.LastDoneCheckpoint + 1)
                {
                    var next = travelActivity.SimplePath[travelActivity.LastDoneCheckpoint + 1];

                    if (_agent.Point.DistanceTo(next) <= _cornerCutOffRadius)
                    {
                        if (travelActivity.LastDoneCheckpoint < travelActivity.SimplePath.Count - 1)
                            travelActivity.LastDoneCheckpoint++;
                    }
                    else
                    {
                        //                        if (travelActivity.LastDoneCheckpoint > 0 && MovementSystem.QueryVisibility(Point, next, 0.1f))
                        //                            travelActivity.LastDoneCheckpoint++;
                    }
                    _agent.DesiredPosition = next;
                }
            }
            else if (_agent.CurrentActivity is QueueActivity)
            {
                var qa = (QueueActivity) _agent.CurrentActivity;

                _agent.DesiredPosition = (qa.Queueable as SpatialQueueablePoi).AskMyPosition(_agent);
            }
            else if (_agent.CurrentActivity is SimpleActionActivity)
            {
                var sa = (SimpleActionActivity) _agent.CurrentActivity;

                //avoid oscilations


                if (_agent.Point.DistanceTo(sa.ActionPoint) < _deadZoneRadius)
                    _agent.DesiredPosition = _agent.Point;
                else
                    _agent.DesiredPosition = sa.ActionPoint;
            }
            else
            {
                _agent.DesiredPosition = _agent.Point;
            }
        }

        #endregion


        public override void StartInteract(IPointOfInterest poi)
        {

        }

        public override void StopInteract(IPointOfInterest poi)
        {

        }
    }
}

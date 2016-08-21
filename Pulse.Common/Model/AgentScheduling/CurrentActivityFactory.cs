using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.AgentScheduling
{
    public class CurrentActivityFactory
    {
        private AbstractPulseAgent _agent;

        public CurrentActivityFactory(AbstractPulseAgent agent)
        {
            _agent = agent;
        }

        public CurrentActivity GetWaitingActivity(TimeSpan timeToWait)
        {
            var a = new WaitingActivity();
            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks);
            a.PlannedEndTime = a.RealStartTime + timeToWait;
            return a;
        }

        public CurrentActivity GetHomeActivity(TimeSpan timeToWait)
        {
            var a = new HomeActivity();
            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks);
            a.PlannedEndTime = a.RealStartTime + timeToWait;
            return a;
        }

//        //TODO refactor end or difference?
//        public CurrentActivity GetRegularTravelingActivity(IPointOfInterest goal)
//        {
//            var a = new SimplePathMovementSystem();
//            a.PointOfInterest = goal;
//            //случайная задержка в несколько секунд (чтобы несколько одинаковых агентов не выходили одновременно)
//            var miniGandigap = new TimeSpan(0, 0, RandomUtil.RandomInt(0, 60));
//            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks) + miniGandigap;
//            return a;
//        }

        public CurrentActivity GetSimpleActionActivity(PlannedActivity plannedActivity)
        {
            // var goalAction = PlannedDailyAgentSchedule.PlannedDailySchedule.Dequeue();
            var a = new SimpleActionActivity();
            a.Building = plannedActivity.Poi;
            a.PlannedActivity = plannedActivity;
            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks);
            a.ActionPoint = ClipperUtil.GetRandomPointOnPolygon(plannedActivity.Poi.Polygon);

            //TODO this is default duration

            TimeSpan duration;
            if (a.PlannedActivity.Activity.TimeDuration != null)
                duration = new TimeSpan(0, a.PlannedActivity.Activity.TimeDuration.GetRandom(), 0);
            else
                duration = new TimeSpan(0, 10, 0);

//            var duration = a.PlannedActivity.PlannedEndTime.Ticks == 0
//                ? new TimeSpan(0, 10, 0)
//                : a.PlannedActivity.PlannedEndTime - a.PlannedActivity.PlannedStartTime;

            a.PlannedEndTime = a.RealStartTime + duration;
            return a;
        }


        //TODO add floor
        public CurrentActivity GetSimpleActionActivity(PlannedActivity first, PulseVector2 goal)
        {
            var v = GetSimpleActionActivity(first);
            (v as SimpleActionActivity).ActionPoint = goal;
            return v;
        }
        

        public CurrentActivity GetComplexTravelingActivity(PulseVector2 point, int level)
        {
            var a = new ComplexTravelingActivity();
            a.Point = point;
            a.Level = level;
            //            a.PointOfInterest = goal;
            //var miniGandigap = new TimeSpan(0, 0, RandomUtil.RandomInt(0, 10*60));
            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks); // + miniGandigap;
            return a;
        }

        public CurrentActivity GetPassiveActionActivity(PlannedActivity plannedActivity)
        {
            // var goalAction = PlannedDailyAgentSchedule.PlannedDailySchedule.Dequeue();
            var a = new PassiveActionActivity();
            a.Building = plannedActivity.Poi;
            a.PlannedActivity = plannedActivity;
            a.RealStartTime = new DateTime(_agent.WorldKnowledge.Clock.Ticks);

            return a;
        }

        public CurrentActivity GetTravelingActivityFromTravelingUnit(ITravelPath travelPath)
        {
            if (travelPath is SimpleTravelPath)
            {
                return new SimplePathTravelingActivity(travelPath as SimpleTravelPath);
            }
            else if (travelPath is PortalTravelPath)
            {
                return new SimplePortalActivity(travelPath as PortalTravelPath);
            }
            else
            {
                return travelPath as CurrentActivity;
            }
        }
    }
}

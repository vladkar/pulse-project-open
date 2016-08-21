using System.Collections.Generic;
using Pulse.Common.Behavior.Pulse;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.Agent
{
    public interface IRole
    {
        string Name { get; }
        int Id { get; }

        AbstractClassSchedule AbstractClassSchedule { get; set; }
        PlannedDailyAgentSchedule PlannedDailyAgentSchedule { get; set; }


        //TODO new
//        IPointOfInterest CurrentPoi { get; set; }
//        IPlanner Planner { get; set; }
//        TestSchedule Schedule { set; get; } // current, done, list
//        Intention CurrentIntention { set; get; }

            TestData Data { set; get; }
        SimpleScheduleRoleConfig Config { get; }

        void CheckSchedule();
        void ProcessActivity();
        PlannedDailyAgentSchedule GetPlannedSchedule();
        IList<AbstractScheduleActivity> GetAbstractSchedule();
        CurrentActivity GetActivity();

        event OnStartInteractPoiDelegate OnStartInteratPoi;
        event OnStopInteractPoiDelegate OnStopInteractPoi;

        void StartInteract(IPointOfInterest poi);
        void StopInteract(IPointOfInterest poi);

        void Initialize();
        void Update();
    }

    public delegate void OnStartInteractPoiDelegate(IPointOfInterest poi);
    public delegate void OnStopInteractPoiDelegate(IPointOfInterest poi);
}

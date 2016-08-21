using System.Collections.Generic;
using NLog;
using Pulse.Common.Behavior.Pulse;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.Agent
{
    public abstract class AbstractAgentRole : IRole
    {
        protected AbstractPulseAgent _agent;

        protected static string _name;
        public string Name { get { return _name; } }
        protected int _id;
        public int Id { get { return _id; } }

        public AbstractClassSchedule AbstractClassSchedule { get; set; }
        public PlannedDailyAgentSchedule PlannedDailyAgentSchedule { get; set; }


        //TODO new
//        public IPointOfInterest CurrentPoi { get; set; }
        public TestData Data { set; get; }
        public SimpleScheduleRoleConfig Config { get; protected set; }

        protected AbstractAgentRole(AbstractPulseAgent agent, string name, int id)
        {
            _agent = agent;
            _name = name;
            _id = id;
        }

        public event OnStartInteractPoiDelegate OnStartInteratPoi;
        public event OnStopInteractPoiDelegate OnStopInteractPoi;

        public abstract void CheckSchedule();
        public abstract PlannedDailyAgentSchedule GetPlannedSchedule();
        public abstract IList<AbstractScheduleActivity> GetAbstractSchedule();
        public abstract CurrentActivity GetActivity();
        public abstract void CheckActivity();
        public abstract void DoActivity();
        public abstract void GetDesiredPosition();

        public abstract void StartInteract(IPointOfInterest poi);
        public abstract void StopInteract(IPointOfInterest poi);

        public virtual void Initialize()
        {
            
        }

        public virtual void Update()
        {
            CheckSchedule();
            ProcessActivity();
            GetDesiredPosition();
        }

        public void ProcessActivity()
        {
            CheckActivity();
            DoActivity();
        }

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}

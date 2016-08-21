using System.Collections.Generic;
using NLog;
using Pulse.Common.DeltaStamp;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{
    public abstract class AbstractPulseAgent : AgentBase, IPluginable, IPseudo3DObject, IPulseAgentDeltaData
    {
        protected AbstractPulseAgent(long id) : base(id)
        {
        }

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public IRole Role { get; set; }
        public virtual PulseVector2 Point { set; get; }
        public PulseVector2 DesiredPosition { set; get; }
        public int Level { get; set; }
        public ISocialEconomyClass SocialEconomyClass { get; set; }
        public IPhysicalCapabilityClass PhysicalCapabilityClass { get; set; }
        public IPointOfInterest Home { get; set; }
        public IPointOfInterest CurrentPointOfInterest { get; set; }
        public abstract bool IsInsideBuilding { get; set; }
        public MovementSystem MovementSystem { set; get; }

        //public AbstractClassSchedule AbstractClassSchedule { get; set; }
        //public PlannedDailyAgentSchedule PlannedDailyAgentSchedule { get; set; }
        public CurrentActivity CurrentActivity { get; set; }

        public IWorldKnowledge WorldKnowledge { get; set; }

        public AgentsFamily Family { set; get; }

        //TODO THINK
        //IDictionary<string, Buff> Buffs { get; set; }

        public PluginsContainer PluginsContainer { get; set; }

        public event OnEnterInBuildingDelegate OnEnterInBuilding;
        public event OnMoveDelegate OnMove;
        public event OnKillDelegate OnKill;
        public event OnBornDelegate OnBorn;
        public event OnOnFloorMoveDelegate OnFloorMove;

        #region event delegates & invokes

        public delegate void OnMoveDelegate(PulseVector2 coord);
        public delegate void OnEnterInBuildingDelegate(IPointOfInterest poi);
        public delegate void OnKillDelegate(string reason);
        public delegate void OnBornDelegate();
        public delegate void OnOnFloorMoveDelegate(int level);

        protected void OnMoveInvoke(PulseVector2 coord)
        {
            var handler = OnMove;
            if (handler != null) handler(coord);
        }

        protected void OnFloorMoveInvoke(int floor)
        {
            var handler = OnFloorMove;
            if (handler != null) handler(floor);
        }

        protected void OnEnterInBuildingInvoke(IPointOfInterest poi)
        {
            var handler = OnEnterInBuilding;
            if (handler != null) handler(poi);
        }

        protected void OnKillInvoke(string reason)
        {
            var handler = OnKill;
            if (handler != null) handler(reason);
        }

        protected void OnBornInvoke()
        {
            var handler = OnBorn;
            if (handler != null) handler();
        }

        #endregion

        public IPseudo3DAgentNavigator Navigator { set; get; }

        public PulseVector2 PrefVelocity { get; set; }


        //TODO india experiment
        public double Pressure { get; set; }
        public double StepDistance { get; set; }
        public Sex Sex { get; set; }
        public PulseVector2 Force { get; set; }

        //


        public abstract void Initialize();

        public abstract void Move(PulseVector2 coord);
        public abstract void DoneActivity();
        public abstract void ChangeLevel(int floor);
        public abstract void Kill(string reason);

        public abstract void StartInteractWithPoi(IPointOfInterest poi);
        public abstract void StopInteractWithPoi();

        public abstract void SetMovementSystem(UnitTravelingActivity activity);

        public IList<ITravelPath> CalculatePath(PulseVector2 startPoint, int startLevel, PulseVector2 destPoint, int destLevel)
        {
            return Navigator.GeneratePath(startPoint, destPoint, startLevel, destLevel);
        }
    }

    public enum Sex { Male, Female}
}
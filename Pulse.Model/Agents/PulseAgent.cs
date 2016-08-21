using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Agents
{
    public class PulseAgent : AbstractPulseAgent
    {
        private MovementSystemFactory _msf;

        public override bool IsInsideBuilding { get; set; }
        
        public IDictionary<string, Buff> Buffs { get; set; }

        public PulseAgent(long id, PulseVector2 point = default(PulseVector2))
            : base(id)
        {
            Point = point;
            PrefVelocity = default(PulseVector2);
        }

        public PulseAgent(long id, MovementSystemFactory msf, PulseVector2 point = default(PulseVector2))
            : this(id, point)
        {
            _msf = msf;
        }

        public override void Initialize()
        {
            InitializePlugins();
        }

        private void InitializePlugins()
        {
            PluginsContainer.Plugins.Values.OfType<PluginBaseAgent>().ToList().ForEach(plgn => plgn.Initialize(this));
        }

        public override void StepAct(double timeStep, double time)
        {
            if (IsAlive)
            {
                UpdatePlugins(timeStep, time);
                UpdateRole();
                UpdateVelocity();
            }
        }

        private void UpdateVelocity()
        {
            //PrefVelocity = new Vector(DesiredPosition - Point).Normalize();

            var X = DesiredPosition.X - Point.X;
            var Y = DesiredPosition.Y - Point.Y;
            var l = Math.Sqrt(X * X + Y * Y);
            if (Math.Abs(l) < 0.0001)
                PrefVelocity = default(PulseVector2);
            else
                PrefVelocity = new PulseVector2(X / l, Y / l);

            if (Double.IsNaN(PrefVelocity.X) || Double.IsNaN(PrefVelocity.Y))
                Debug.Assert(true);
        }

        public override void SetMovementSystem(UnitTravelingActivity activity)
        {
            MovementSystem?.RemoveAgent(this);
            MovementSystem = _msf.RegisterAgent(this, CurrentActivity as UnitTravelingActivity);
            OnKill += (reason) => MovementSystem.RemoveAgent(this);
        }

        private void UpdateRole()
        {
            Role.Update();
        }

        private void UpdatePlugins(double timeStep, double time)
        {
            foreach (var plugin in PluginsContainer.Plugins)
            {
                var up = plugin.Value as IUpdatablePlugin;
                if (up != null)
                    up.Update(timeStep, time);
            }
            //PluginsContainer.Plugins.Values.OfType<IUpdatablePlugin>().ToList().ForEach(plgn => plgn.Update(timeStep, time));
        }

        public override void Move(PulseVector2 coord)
        {
            Point = coord;

            if (CurrentPointOfInterest != null && CurrentPointOfInterest.TravelPoint != Point)
            {
                var building = CurrentPointOfInterest as IEnterable;
                building?.Exit(this);
            }

            OnMoveInvoke(coord);
        }

        public override void DoneActivity()
        {
            CurrentActivity.Finish();
        }

        public override void ChangeLevel(int floor)
        {
            ////// ok for multilevel sf movement system, bad for simple movement system
            MovementSystem?.RemoveAgent(this);

            Level = floor;

            OnFloorMoveInvoke(floor);

            ////// same
            if (MovementSystem!=null) SetMovementSystem(null);
        }

        public override void Kill(string reason)
        {
            OnKillInvoke(reason);
            TerminationReasonInfo = reason;
            IsAlive = false;
        }

        public override void StartInteractWithPoi(IPointOfInterest poi)
        {
            var interactable = poi as IInteractable;
            if (interactable == null) return;
            
            //Point = poi.TravelPoint;
            OnEnterInBuildingInvoke(poi);
            interactable.Enter(this);
        }

        public override void StopInteractWithPoi()
        {
            throw new NotImplementedException();
        }

        public void Waiting()
        {
        }

        public void Do()
        {
        }

        public override string ToString()
        {
            return $"ID: {Id}, floor: {Level}, point: {Point}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

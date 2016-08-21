using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Model.AgentScheduling.Current.Traveling
{
    public class SimplePortalActivity : UnitTravelingActivity
    {
        public ILevelPortal Enter { set; get; }
        public ILevelPortal Exit { set; get; }

        public SimplePortalActivity() { }

        public SimplePortalActivity(PortalTravelPath unit) : this()
        {
            Enter = unit.Enter;
            Exit = unit.Exit;
        }
    }
}
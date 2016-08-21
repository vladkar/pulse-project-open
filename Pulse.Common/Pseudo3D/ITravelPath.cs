namespace Pulse.Common.Pseudo3D
{
    public interface ITravelPath
    {
        bool Finished();
    }


    public class PortalTravelPath : ITravelPath
    {
        public ILevelPortal Enter { set; get; }
        public ILevelPortal Exit { set; get; }
        public LevelPortalTransporter Transporter { set; get; }

        public bool Finished()
        {
            throw new System.NotImplementedException();
        }
    }
}
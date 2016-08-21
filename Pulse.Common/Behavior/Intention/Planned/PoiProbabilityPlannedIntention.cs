namespace Pulse.Common.Behavior.Intention.Planned
{
    #region planned

    public class PoiProbabilityPlannedIntention : IPoiPlannedIntension, IProbabilityPlannedIntension
    {
        public string Name { get; set; }
        public string[] PoiTypes { get; set; }
        public double Probability { get; set; }
    }

    #endregion


    #region current

    #endregion
}

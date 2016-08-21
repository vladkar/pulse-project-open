using System.Collections.Generic;

namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class InfectionInfo
    {
        public string Name { set; get; }
        public int MatrixPhaseUpdatePeriodMinutes { set; get; }

        public IDictionary<string, InfectionTransmission> InfectionTransmissionTypes { set; get; }
    }
}
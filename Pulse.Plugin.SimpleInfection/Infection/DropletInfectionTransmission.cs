namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class DropletInfectionTransmission : InfectionTransmission
    {
        public int NoSymptomsRadius { set; get; }
        public int SymptomsRadius { set; get; }
        public int ContactUpdatePeriodMinutes { set; get; }
        public int PoissonMeanMinutes { set; get; }
    }
}
using System;
using System.Collections.Generic;

namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class BaseInfectionStage
    {
        public enum InfectionStates { IM = 1, S = 2, E = 3, I1 = 4, I2 = 5, T = 6, R = 7, C = 8 }

        public TimeSpan Min { set; get; }
        public TimeSpan Max { set; get; }
        public DateTime StartTime { set; get; }
        public InfectionStates InfectionState { set; get; }
        public IList<Tuple<string, int>> ChangeStageProbabilities { set; get; }

//        public bool Immunuty { set; get; }

        public BaseInfectionStage() { }

        public BaseInfectionStage(BaseInfectionStage infectionStage)
        {
            InfectionState = infectionStage.InfectionState;
            Min = infectionStage.Min;
            Max = infectionStage.Max;
//            Immunuty = infectionStage.Immunuty;
        }

    }
}

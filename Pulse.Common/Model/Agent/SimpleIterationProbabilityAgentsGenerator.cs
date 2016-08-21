using System;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace Pulse.Common.Model.Agent
{
    public abstract class SimpleIterationProbabilityAgentsGenerator : AbstractExternalPortalAgentGenerator
    {
        public double P { get; set; }
        public double Count { get; set; }
        public static double K { set; get; } = 1;

        protected SimpleIterationProbabilityAgentsGenerator(IExternalPortal externalPortal, double p, double count = -1) : base(externalPortal)
        {
            P = p;
            Count = count;
        }


        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (Count > 0)
            {
                var perStep = P*K;

                for (var i = 0; i < (int) perStep; i++)
                {
                    CreateAgent();
                    Count--;
                }

                if (RandomUtil.PlayProbability(perStep - (int)perStep))
                {
                    CreateAgent();
                    Count--;
                }
            }
        }

        protected abstract AbstractPulseAgent CreateAgent();
    }
}
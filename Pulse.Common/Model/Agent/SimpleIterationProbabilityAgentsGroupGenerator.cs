using System;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace Pulse.Common.Model.Agent
{
    public abstract class SimpleIterationProbabilityAgentsGroupGenerator : AbstractExternalPortalAgentGenerator
    {
        public double P { get; set; }
        public double Count { get; set; }
        public static double K { set; get; } = 1;
        public int MaxGroupSize { get; set; }
        public int MinGroupSize { get; set; }

        protected SimpleIterationProbabilityAgentsGroupGenerator(IExternalPortal externalPortal, int minGroupSize, int maxGroupSize, double p, double count = -1) : base(externalPortal)
        {
            P = p;
            Count = count;
            MinGroupSize = minGroupSize;
            MaxGroupSize = maxGroupSize;
        }



        public override void Update(double timeStep, double time, DateTime geotime)
        {
//            if (Count > 0)
            {
                var perStep = P*K;
                
                var grsize = RandomUtil.RandomInt(MinGroupSize, MaxGroupSize+1);

                if (Count > 0)
                {
                    for (var i = 0; i < (int) perStep; i++)
                    {
                        CreateGroup(grsize);
                        Count -= grsize;
                    }

                    if (RandomUtil.PlayProbability(perStep - (int) perStep))
                    {
                        CreateGroup(grsize);
                        Count -= grsize;
                    }
                }
            }
        }

        protected abstract void CreateGroup(int grsize);
    }
}
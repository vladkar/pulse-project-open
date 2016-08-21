using System.Collections.Generic;
using Pulse.Common.Model.Agent;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.Model.Agents
{
    public class SimpleAgentsGenerator : IPulseAgentGenerationModel
    {
        private LinkedList<Pair<double, AgentBase>> Buffer;
        public PopulationManager PopulationManager { get; set; }

        public SimpleAgentsGenerator(PopulationManager pm, int initPopulation)
        {
            PopulationManager = pm;
            Buffer = new LinkedList<Pair<double, AgentBase>>();
            LoadAgents(initPopulation);
        }

        public void LoadAgents(int initPopulation)
        {
//            PopulationManager.InitPopulation(initPopulation);

            foreach (var a in PopulationManager.Population)
            {
                Buffer.AddFirst(new Pair<double, AgentBase>(0, (AgentBase)a)); //unsafe cast
            }
        }
        
//        public void AddAgentAtHome()
//        {
//            var newAgent = PopulationManager.CreateAgentAtHome();
//            Buffer.AddLast(new Pair<double, AgentBase>(0, newAgent));
//        }

//        public void AddAgentAtHome(IPointOfInterestcs home)
//        {
//            var newAgent = PopulationManager.CreateAgentAtHome();
//            newAgent.Home = home;
//            Buffer.AddLast(new Pair<double, AgentBase>(0, newAgent));
//        }

        public AgentBase[] GenerateAgentsInTime(double simTime)
        {
            var ret = new List<AgentBase>();

            bool stop = Buffer.Count == 0;

            while (!stop)
            {
                if (Buffer.Count == 0)
                    break;

                if (Buffer.First.Value.First <= simTime)
                {
                    ret.Add(Buffer.First.Value.Second);
                    Buffer.RemoveFirst();
                }
                else
                {
                    stop = true;
                }
            }

            return ret.ToArray();
        }

        public bool HaveMoreAgents(double currentSimTime)
        {
            if (Buffer != null)
                return Buffer.Count != 0;

            return false;
        }

        public AbstractPulseAgent CreateNewAgent()
        {
            return PopulationManager.CreateAgent();
        }
    }
}

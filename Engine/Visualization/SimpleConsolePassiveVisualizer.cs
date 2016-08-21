//using System;
//using System.Collections.Generic;
//using MultiagentEngine.Pulse.Agents;
//using MultiagentEngine.Pulse.Containers;
//using MultiagentEngine.Pulse.Engine;
//using MultiagentEngine.Pulse.Stats;
//using NLog;
//
//namespace MultiagentEngine.Pulse.Visualization
//{
//    public class SimpleConsolePassiveVisualizer : IPassiveVisualizer
//    {
//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
//
//        public void Initialize()
//        {
//            Log.Info("Console visualizer initialization");
//        }
//
//        public void Dispose()
//        {
//            Log.Info("Console visualizer disposed");
//        }
//
//        public void Update(SimulationEngine engine)
//        {
//            RenderStep(engine.World, engine.Info, engine.Stats);
//        }
//
//        //public void RenderStep(SimWorld world, SimulationInfo info, AgentsAggregatedStatistics stats)
//        public void RenderStep(SimWorld world, SimulationInfo info, IDictionary<string, string> stats)
//        {
////            Console.WriteLine("===== Information =====");
////            foreach (var pair in info)
////            {
////                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
////            }
//
//            Console.WriteLine("===== Statistics =====");
//            foreach (var pair in stats)
//            {
//                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
//            }
//
//            Console.WriteLine("===== Agents =====");
//            foreach (var agent in world.Agents.OfType<SpatialAgentBase>())
//            {
//                Console.WriteLine("Agent {0}: {1}", agent.Id, agent.Position);
//            }
//
//        }
//
//        public void Finalization()
//        {
//            
//        }
//    }
//}

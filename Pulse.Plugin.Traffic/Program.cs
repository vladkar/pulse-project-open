using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddableTraffic;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Utils.Algorithms.Astar;
using Pulse.Common.Utils.Graph;

namespace Pulse.Plugin.Traffic
{
    class Program
    {
        static void Main(string[] args)
        {
            // General configuration parameters
            var config = new EmbeddableTrafficEngineConfig
            {
                RoadGraphFilePath = "zsd.XML",
                SimulationStartsAt = 0,
                TimeStep = 0.1
            };

            // Constuctor
            var engine = new EmbeddableTrafficEngine();

            // Initialize with parameters
            engine.InitializeEngine(config);

            // CLI
            bool stop = false;
            int idCounter = 0;
            Console.WriteLine("  Usage: n - next iteration, a - add agent, p - get positions, q - quit");
            while (!stop)
            {
                // Prompt
                Console.Write(">");
                string cmd = Console.ReadLine().Trim();

                switch (cmd)
                {
                    case "n":
                        Console.WriteLine("  Step number: {0}", engine.Info.StepNumber);
                        engine.Step();
                        foreach (var ag in engine.AgentsFinishedAfterStep)
                        {
                            Console.WriteLine("  Finished agents: {0}", String.Join(",", ag.PassengersExternalIds));
                        }
                        break;
                    case "a":
                        var ags = new List<VehicleAgent>();
                        Random rnd = new Random((int)DateTime.UtcNow.ToFileTime());
                        for (int i = 0; i < 3; i++)
                        {
                            var lat = rnd.NextDouble() * (60.1059 - 59.7399) + 59.7399;
                            var lon = rnd.NextDouble() * (30.5967 - 29.6066) + 29.6066;
                            var lat1 = rnd.NextDouble() * (60.1059 - 59.7399) + 59.7399;
                            var lon1 = rnd.NextDouble() * (30.5967 - 29.6066) + 29.6066;

                            var v = new VehicleAgent
                            {
                                Origin = engine.GetCoordinateProjection(new GeoCoordinate(lat, lon)),
                                Destination = engine.GetCoordinateProjection(new GeoCoordinate(lat1, lon1)),
                                PassengersExternalIds = new[] { idCounter++.ToString() }
                            };

                            ags.Add(v);
                        }
                        engine.AddNewAgents(ags);
                        break;

                    case "p":
                        var ps = engine.GetAllExternalAgentsPositions();
                        Console.WriteLine(String.Join("\n  ", ps.Keys.Select(s => String.Format("{0}: {1}", s, ps[s]))));
                        break;
                    case "q":
                        engine.Finalize();
                        stop = true;
                        break;
                    default:
                        Console.WriteLine("  Usage: n - next iteration, a - add agent, p - get positions, q - quit");
                        break;
                }
            }
        }
    }
}

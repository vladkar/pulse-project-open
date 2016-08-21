using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading;
using City.ControlsServer;
using System.Windows.Forms;
using Fusion;
using Fusion.Build;
using Fusion.Core.Development;
using Fusion.Engine.Common;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Pulse.Common.Model.Agent;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Map;
using Pulse.Scenery.Krestovsky.AgentGeneration;

namespace City
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // 	colored console output :
            Log.AddListener(new ColoredLogListener());
            Log.AddListener(new LogRecorder());
            Log.VerbosityLevel = LogMessageType.Verbose;
            //TraceRecorder.MaxLineCount = 5;
            //
            //	Build content on startup :
            //
            try
            {
                Builder.Options.InputDirectory = @"..\..\..\Content";
                Builder.Options.TempDirectory = @"..\..\..\Temp";
                Builder.Options.OutputDirectory = @"Content";
                Builder.SafeBuild();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return 1;
            }


            //
            //	Run game :
            //
            using (var engine = new Game("City"))
            {
                //	create SV, CL and UI instances :
                engine.GameServer = new PulseMasterServer(engine);
                engine.GameClient = new PulseMasterClient(engine);
                engine.GameInterface = new CustomGameInterface(engine);

                //	load configuration.
                //	first run will cause warning, 
                //	because configuration file still does not exist.
				engine.RenderSystem.Fullscreen = false;
				engine.LoadConfiguration("Config.ini");

				//	apply command-line options here:
				//	...
                if(!CustomGameInterface.IsDemo)
				    LaunchBox.Show(engine, "Config.ini");
				//LaunchBox

				//	run:
				engine.Run();

				//	save configuration :
				//engine.RenderSystem.Fullscreen = false;
				engine.SaveConfiguration("Config.ini");

			}

			return 0;
        }

        public class StatWriter
        {
            public void Write()
            {
                Realfunc();
            }

            public Action Realfunc { get; set; }
        }

        static int Main2(string[] atgs)
        {
            var p1 = System.Runtime.GCSettings.IsServerGC;
            var p2 = System.Runtime.GCSettings.LatencyMode;

            //System.Runtime.GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;


//            var s = new PulseServer(1);
//            s.Initialize();
//            s.OnStep += IndiaE1(s, dir, 0, 1);
//            s.LoadLevel(new ControlInfo { Id = 1, Name = GlobalStrings.PulseAgentControl, Scenario = "krestovsky" });


            var dir = @"D:\GDrive\2015.12.27_India\report\experiment\e_square_4";

            for (int i = 1; i <= 15; i++)
            {
                KrestovskySimpleStadiumAgentsGenerator.Prob = 0.10 + ((double)i-1)/100;
                
                for (int j = 3; j <= 3; j++)
                {
                    var s = new PulseServer(i*10 + j);
                    s.Initialize(new PulseMasterServer(new Game("City")));

                    var sw = new StatWriter();
                    var i1 = i;
                    var j1 = j;
                    sw.Realfunc = () => { IndiaE1(s, dir, i1, j1); };
                    s.OnStep += sw.Write;

                    s.LoadLevel(new FpsControlInfo { Id = i, Name = GlobalStrings.PulseAgentControl, Scenario = "krestovsky" });

                    //Thread.Sleep(1000);
                    while (s.GetIterations() < 12000)
                    {
                        Thread.Sleep(100);
                    }

                    s.UnloadLevel();
                    Thread.Sleep(1000);
                }
            }

            return 0;
        }


        //TODO remove this method
        private static void IndiaE1(PulseServer s, string dir, int exp, int subExp)
        {
            if (s.GetIterations() % 10 == 0)
            {
                var fDir = $@"{dir}\exp_{exp.ToString("00")}";
                var exists = System.IO.Directory.Exists(fDir);

                if (!exists)
                    System.IO.Directory.CreateDirectory(fDir);

                var path = $@"{fDir}\e_{exp.ToString("00")}_{subExp.ToString("00")}.csv";
                using (var sw = File.AppendText(path))
                {
                    var min = new PulseVector2(2149.455 - 10, -12299.702 - 10) - new PulseVector2(1500, -13600);
                    var max = new PulseVector2(2149.455 + 10, -12299.702 + 10) - new PulseVector2(1500, -13600);
                    
                    var targetAgents = s.GetAgents().Where(a => a.X < max.X && a.Y > min.Y && a.X > min.X && a.Y < max.Y)
                            .ToArray();

                    var val = targetAgents.Length.ToString();
                    var pagents = targetAgents.OfType<ISfAgent>();

                    var acount = pagents.Count();
                    var press = acount > 0 ? pagents.Average(a => a.Pressure) : 0;
                    var dist = acount > 0 ? pagents.Where(a => a.StepDist > 0).Average(a => a.StepDist) : 0;

                    sw.WriteLine($"{s.GetIterations()},{val},{press},{dist}");

                    Console.Out.WriteLine($"Exp Ser: {exp}, sub exp: {subExp}, iter: {s.GetIterations()}");
                    Console.Out.WriteLine($"Agent in square: {val}, avg press: {press}, avg dist: {dist}");
                }
            }
        }



        static void Main3(string[] atgs)
        {
            var p1 = System.Runtime.GCSettings.IsServerGC;
            var p2 = System.Runtime.GCSettings.LatencyMode;

            //System.Runtime.GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;


            //            var s = new PulseServer(1);
            //            s.Initialize();
            //            s.OnStep += IndiaE1(s, dir, 0, 1);
            //            s.LoadLevel(new ControlInfo { Id = 1, Name = GlobalStrings.PulseAgentControl, Scenario = "krestovsky" });


            var globalDir = @"D:\vkfest_stage_test_experiment_1";
            var now = DateTime.Now;
            var dir = "exp-" + now.ToString("yyyy-dd-M--HH-mm-ss");

            var sch = new {af = 0.12, am = 70, leader_dist = 0.5, probe_force = 2.0};

            var afs = new[] {0.12};
            var ams = new[] { 70.0 };
            var leader_dists = new[] { 0.1, 0.3, 0.5, 0.75, 1, 1.5, 2, 3 };
            var probe_forces = new[] { 0.5, 1, 2, 3, 5, 10, 15, 20 };
            var ns = 1;

            var count = 0;
            foreach (var af in afs)
            {
                foreach (var am in ams)
                {
                    foreach (var leaderDist in leader_dists)
                    {
                        foreach (var probeForce in probe_forces)
                        {
                            for (var n = 1; n < ns+1; n++)
                            {
                                var exists = System.IO.Directory.Exists($@"{globalDir}\{dir}");
                                if (!exists) System.IO.Directory.CreateDirectory($@"{globalDir}\{dir}");

                                var file = $@"{ globalDir}\{dir}\exp__af_{String.Format("{0:N2}", af)}__am_{String.Format("{0:N2}", am)}__leaderDist_{String.Format("{0:N2}", leaderDist)}__probeForce_{String.Format("{0:N2}", probeForce)}__n_{n.ToString("00")}.csv";
//                                var path = $@"{dir}\{file}";

                                PulseServer s;
                                using (var filewr = File.AppendText(file))
                                {
                                    SfMultilevelMovementSystem.ExpCoef = (float) probeForce;
                                    s = new PulseServer(++count);
                                    s.Initialize(new PulseMasterServer(new Game("City")));

                                    var sw = new StatWriter();
                                    sw.Realfunc = () => { VkFestTestStageExp1(s, filewr); };
                                    s.OnStep += sw.Write;


                                    s.LoadLevel(new FpsControlInfo
                                    {
                                        Id = count,
                                        Name = GlobalStrings.VkFestControl,
                                        Scenario = "VKFestStage"
                                    });

                                    //Thread.Sleep(1000);
                                    while (s.GetIterations() < 2000)
                                    {
                                        Thread.Sleep(100);
                                    }
                                }

                                s.UnloadLevel();
                                Thread.Sleep(1000);

                            }
                        }
                    }
                }
            }
        }

        private static void VkFestTestStageExp1(PulseServer s, StreamWriter sw)
        {
            if (s.GetIterations()%10 == 0)
            {
//                var astf = s.GetAgetnEngine();
                var agents = s.GetAgents().OfType<ISfAgent>();
                var probes = agents.Where(a => a.Role == 140).ToArray();
                
                var prs = new List<double>();
                
                sw.Write($"{s.GetIterations()}, ");
                foreach (var probe in probes)
                {
                    prs.Add(probe.Pressure);
                    sw.Write($"{probe.Id}, ");
                    sw.Write($"{probe.Pressure}, ");
                    sw.Write($"{probe.X}, ");
                    sw.Write($"{probe.Y}, ");
                }
                var apr = prs.Average();
                sw.Write($"{apr}");
                sw.WriteLine();
            }
        }
    }
}

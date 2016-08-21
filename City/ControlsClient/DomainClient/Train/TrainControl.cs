using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using City.ModelServer;
using City.Panel;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Infection;

namespace City.ControlsClient.DomainClient.Train
{
	public class TrainControl : PulseMicroModelControl
	{
        //TODO remove static!!!
        public static DateTime Time;

        public TrainRoute CurrentRoute { set; get; }
		public Dictionary<string, TrainRoute> TrainRoutes { set; get; }

		string currentScenario = "";
		public DateTime CurrentTime;
		TimeSpan TimeStep = TimeSpan.FromMinutes(1);

		//		private TrainMapView	mapView;
		//		private TrainModelView	modelView;
		//		private TrainGraphics	graphicView;

		private Action	OnServerShutDown;
		private Action	RestartScenario;


		public event Action<StopPlace> OnTrainArrive;
		public event Action<StopPlace> OnTrainDepart;


		private StopPlace lastPlaceCheck = null;


		protected override void InitializeControl()
		{
            var modelView = new TrainModelView(this);
            var mapView = new TrainMapView(this);
            var graphicView = new TrainGraphics(this);
            var devView = new PulseMicroModelDeveloperView(this);

            Views[1] = mapView;
//            Views[1] = devView;
            Views[2] = graphicView;
            Views[3] = modelView;

            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            var route1 = TrainRoute.LoadFromFiles(Game, $"{dir}/Train/routes/ScheduleAdlerMoscow.rrt",
                $"{dir}/Train/routes/RouteAdlerMoscow.txt");
	        var route2 = TrainRoute.LoadFromFiles(Game, $"{dir}/Train/routes/ScheduleAdlerPerm.rrt",
                $"{dir}/Train/routes/RouteAdlerPerm.txt");
	        var route3 = TrainRoute.LoadFromFiles(Game, $"{dir}/Train/routes/ScheduleAdlerMinsk.rrt",
                $"{dir}/Train/routes/RouteAdlerMinsk.txt");

			TrainRoutes = new Dictionary<string, TrainRoute>();

			TrainRoutes.Add("TrainMoscow",	route1);
			TrainRoutes.Add("TrainPerm",	route2);
			TrainRoutes.Add("TrainMinsk",	route3);


			route1.MaxCameraDistance	= 4050;
			route1.FullRouteViewYaw		= 0.678166698529108;
			route1.FullRouteViewPitch	= -0.863416886751499;

			route2.MaxCameraDistance	= 4500;
			route2.FullRouteViewYaw		= 0.797127614860768;
			route2.FullRouteViewPitch	= -0.887396194256949;

			route3.MaxCameraDistance	= 3500;
			route3.FullRouteViewYaw		= 0.614040142060397;
			route3.FullRouteViewPitch	= -0.852883318498561;


			OnTrainArrive += (p) => {
				Console.WriteLine("Train just arrived to: " + p.Title);
			};
			OnTrainDepart += (p) => {
				Console.WriteLine("Train just departed from: " + p.Title);
			};

            if(CustomGameInterface.IsDemo)
			    Game.RenderSystem.Fullscreen = true;

			OnServerShutDown = () => {
				if(currentScenario != "")
					Game.Invoker.PushAndExecute("map " + currentScenario);

				Game.GameServer.OnServerShutdown -= OnServerShutDown;
				Game.Touch.Tap += (Game.GameInterface as CustomGameInterface).TapHandler;
			};

			RestartScenario = () => {
				Game.Touch.Tap -= (Game.GameInterface as CustomGameInterface).TapHandler;
				Game.GameServer.OnServerShutdown += OnServerShutDown;

				Game.Invoker.PushAndExecute("killServer");
			};
		}


	    protected override void LoadControl(ControlInfo controlInfo)
	    {
			currentScenario = controlInfo.Scenario;

			// Choose rout according to scenario
			if (controlInfo.Scenario.Contains("perm"))
					CurrentRoute = TrainRoutes["TrainPerm"];
            else if (controlInfo.Scenario.Contains("minsk"))
                CurrentRoute = TrainRoutes["TrainMinsk"];
            else if (controlInfo.Scenario.Contains("moscow"))
                CurrentRoute = TrainRoutes["TrainMoscow"];
            else	CurrentRoute = TrainRoutes["TrainMinsk"];

	        Time = CurrentTime = CurrentRoute.Schedule.StopsPlacesList.First().ArrivalTime;

//			var panel = mapView.Panel as GisPanel;
//			panel.PanelLayer.GisLayers.Add(CurrentRoute.LineLayer); 
//			panel.PanelLayer.GisLayers.Add(CurrentRoute.PolyRouteLayer);

		    lastPlaceCheck = null;

			base.LoadControl(controlInfo);
        }


	    protected override void UpdateControl(GameTime gameTime)
	    {
			//if (!Game.Keyboard.IsKeyDown(Keys.Space)) CurrentTime += new TimeSpan((long) (TimeStep.Ticks*gameTime.ElapsedSec));
			//if (Game.Keyboard.IsKeyDown(Keys.Q)) CurrentTime = CurrentRoute.Schedule.StopsPlacesList[0].ArrivalTime;

			Time = CurrentTime = ModelTime;

		    var placeCheck = CurrentRoute.IsTrainOnStation(CurrentTime);

		    if (placeCheck != null && lastPlaceCheck == null) {
			    OnTrainArrive?.Invoke(placeCheck);
		    }
		    if (placeCheck == null && lastPlaceCheck != null) {
			    OnTrainDepart?.Invoke(lastPlaceCheck);
		    }

		    lastPlaceCheck = placeCheck;

			base.UpdateControl(gameTime);


			if(CurrentTime > CurrentRoute.Schedule.StopsPlacesList.Last().ArrivalTime + TimeSpan.FromHours(72)) {
				RestartScenario();
            }
	    }
        

		public override void UnloadControl()
		{
//			var panel = mapView.Panel as GisPanel;
//			panel.PanelLayer.GisLayers.Remove(CurrentRoute.LineLayer);
//			panel.PanelLayer.GisLayers.Remove(CurrentRoute.PolyRouteLayer);
		}


	    private DateTime prevTime;
        public Dictionary<int, List<Vector2>> GetInfectionPlotData()
        {
            var numberGraphics = 1;

            var im = (int)AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.IM);
            var sa = (int)AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.S);
            var e = (int)AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.E);
            var i2 = (int)AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.I2);

            if (!prevTime.Equals(this.ModelTime))
            {
                prevTime = this.ModelTime;
                i++;
                var timeMilliSeconds = (float)ModelTime.Subtract(Plot.MinDate).TotalMilliseconds;
                return new Dictionary<int, List<Vector2>>()
                {
                    //                { 0, new List<Vector2> { new Vector2(i, (int) AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.IM)) } },
                    //                { 1, new List<Vector2> { new Vector2(i, (int) AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.S)) } },
                    //                { 2, new List<Vector2> { new Vector2(i, (int) AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.E)) } },
                    //                { 3, new List<Vector2> { new Vector2(i, (int) AgentsInfected.Count(s => s == BaseInfectionStage.InfectionStates.I2)) } }

                

                    {(int)BaseInfectionStage.InfectionStates.IM, new List<Vector2> {new Vector2(timeMilliSeconds, im)}},
                    {(int)BaseInfectionStage.InfectionStates.S, new List<Vector2> {new Vector2(timeMilliSeconds, sa)}},
                    {(int)BaseInfectionStage.InfectionStates.E, new List<Vector2> {new Vector2(timeMilliSeconds, e)}},
                    {(int)BaseInfectionStage.InfectionStates.I2, new List<Vector2> {new Vector2(timeMilliSeconds, i2)}},
                };
            }
            else
                return null;
        }


        private int i = 1;
	    public Dictionary<int,List<Vector2>> getDataForGraphic()
	    {
	        var numberGraphics = 1;
	        var points = new List<Vector2>()
	        {
	            new Vector2(DateTime.Now.Millisecond,  (float) (Math.Sin(i))),
            };
	        i++;
            return new Dictionary<int, List<Vector2>>()
            {
                {
                    numberGraphics, points
                }
            };
	    }
    }
}

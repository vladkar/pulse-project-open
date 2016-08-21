using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;

namespace City.ControlsClient.DomainClient
{
    public class PulseMicroModelBeautyView : AbstractPulseView<PulseMicroModelControl>
    {
        private ModelLayer _agentLayer;
        private ModelLayer _objectLayer;
        //private TilesGisLayer tiles;

        private IDictionary<int, Scene>		levelGrounds;
        private IDictionary<int, Vector3[]> earthPoints;
        private ControlInfo config;


        //TODO extract UI logic to base class
        private PulseAgentUI _aui;
        private ICommandSnapshot _currentCommand = null;


		PolyGisLayer		ujjainBM;
		TilesGisLayer		tiles;
		GeoGrid4KmTo250m	geoGrid;

		SpriteLayer bannersLayer;
		GeoBanner[] banners;


		#region Stuff for saving agents

	    private MemoryStream memBuf;
	    private BinaryWriter writer;

		#endregion


		public PulseMicroModelBeautyView(PulseMicroModelControl pulseMicroModelControl)
        {
            Control = pulseMicroModelControl;
        }


        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeView()
        {
        }

        protected override void LoadView(ControlInfo controlInfo)
        {
            config = controlInfo;

            DefautCameraPosition(config.Scenario);

            _agentLayer = GetAgentsLayer(controlInfo.Scenario);
            _agentLayer.IsVisible = true;

			//_objectLayer = GetObjectLayer(controlInfo.Scenario);

            Layers.Add(new GisLayerWrapper(_agentLayer));
            //Layers.Add(new GisLayerWrapper(_objectLayer));
			if (controlInfo.Scenario == "mahakaltemple")	Layers.Add(new GisLayerWrapper(GetObjectLayer("mahakaltemple")));
			if (controlInfo.Scenario == "harsiddhitemple")	Layers.Add(new GisLayerWrapper(GetObjectLayer("harsiddhitemple")));

			levelGrounds	= GetGround(controlInfo.Scenario);
			earthPoints		= new Dictionary<int, Vector3[]>();
			
			foreach (var levelGround in levelGrounds) {
                var earthGround = levelGround.Value;
			
                var transforms = new Matrix[earthGround.Nodes.Count];
                earthGround.ComputeAbsoluteTransforms(transforms);
			
				earthPoints.Add(levelGround.Key, GetMeshPointsTransformed(earthGround.Meshes[0].Vertices, transforms[1]));
            }


			ujjainBM = PolyGisLayer.CreateFromUtmFbxModel(Game, "India/ujjain_577340,625_2572949,5_43N");
			ujjainBM.IsVisible = true;
			ujjainBM.ZOrder = 1;
			Layers.Add(new GisLayerWrapper(ujjainBM));
			
			var ram = PolyGisLayer.CreateFromUtmFbxModel(Game, "India/ramghat_577340,625_2572949,5_43N");
			ram.IsVisible = true;
            Layers.Add(new GisLayerWrapper(ram));
			
			var res = PolyGisLayer.CreateFromUtmFbxModel(Game, "India/researchcamp_577340,625_2572949,5_43N");
			res.IsVisible = true;
            Layers.Add(new GisLayerWrapper(res));

			geoGrid = new GeoGrid4KmTo250m(Game, new DVector2(75.6, 23.02));
			Layers.Add(new GisLayerWrapper(geoGrid.LinesGrid4km));
			Layers.Add(new GisLayerWrapper(geoGrid.LinesGrid2km));
			Layers.Add(new GisLayerWrapper(geoGrid.LinesGrid1km));
			Layers.Add(new GisLayerWrapper(geoGrid.LinesGrid500m));
			Layers.Add(new GisLayerWrapper(geoGrid.LinesGrid250m));


			//tiles = new TilesGisLayer(Game, ViewLayer.GlobeCamera);
			//tiles.IsVisible = false;
			//Layers.Add(new GisLayerWrapper(tiles));

			Game.Keyboard.FormKeyPress += (sender, args) => {
				System.Console.WriteLine(args.KeyChar);
				if (args.KeyChar == 'o') {
					ViewLayer.GlobeCamera.SaveCurrentStateToFile();
				}
				if (args.KeyChar == 'p') {
					ViewLayer.GlobeCamera.LoadAnimation();
				}
				if (args.KeyChar == 'l') {
					ViewLayer.GlobeCamera.StopAnimation();
				}
			};


			//bannersLayer = new SpriteLayer(Game.RenderSystem, 1024);
			//Layers.Add(new SpriteLayerWrapper(bannersLayer));
			//
			//banners = new []
			//{
			//	new GeoBanner(Game, DMathUtil.DegreesToRadians(new DVector2(75.76653, 23.181340)), "")
			//};

			//memBuf = new MemoryStream();
			//writer = new BinaryWriter(memBuf);
			//writer.Write(counter);
        }
        

        private IDictionary<int, Scene> GetGround(string scenario)
        {
            var levelGrounds = new Dictionary<int, Scene>();

            for (int i = 0; i < 10; i++) {
                var possibleLevel = $"Data/{scenario}/vis/earth_ground_0{i}";
                if (Game.Content.Exists(possibleLevel)) {
                    levelGrounds[i] = Game.Content.Load<Scene>(possibleLevel);
                }
            }

            return levelGrounds;
        }

        private ModelLayer GetObjectLayer(string scenario)
        {
			switch (scenario)
			{
				case "harsiddhitemple":
					return new ModelLayer(Game, new DVector2(75.7643192000189, 23.1842322999987), $"Data/{scenario}/vis/object")
					{
						ZOrder		= 1500,
						ScaleFactor = 1,
						XRay		= true,
						Yaw			= MathUtil.PiOverTwo
					};
				case "mahakaltemple":
					return new ModelLayer(Game, new DVector2(75.76653, 23.181340), $"Data/{scenario}/vis/object")
					{
						ZOrder		= 1500,
						ScaleFactor = 1,
						XRay		= true,
						Yaw			= MathUtil.PiOverTwo
					};
				default:
					return null;
			}
        }

		private ModelLayer GetAgentsLayer(string scenario)
		{
			switch (scenario)
			{
				case "harsiddhitemple":
					return new ModelLayer(Game, new DVector2(75.7643192000189, 23.1842322999987), "human_agent", 10000)
					{
						ZOrder		= 1500,
						ScaleFactor = 1
					};
				case "mahakaltemple":
					return new ModelLayer(Game, new DVector2(75.76653, 23.181340), "human_agent", 10000)
					{
						ZOrder = 1500,
						ScaleFactor = 1
					};
                case "vkfeststage":
                    return new ModelLayer(Game, new DVector2(75.76653, 23.181340), "human_agent", 10000)
                    {
                        ZOrder = 1500,
                        ScaleFactor = 1
                    };
                //                case "simpletrain":
                //                    return new ModelLayer(Game, new DVector2(75.76653, 23.181340), "human_agent", 10000)
                //                    {
                //                        ZOrder = 1500,
                //                        ScaleFactor = 1
                //                    };
                default:
					return null;
			}
		}

        private void DefautCameraPosition(string scenario)
        {
            switch (scenario)
            {
                case "harsiddhitemple":
                    ViewLayer.GlobeCamera.Yaw = 1.3223416671447734;
                    ViewLayer.GlobeCamera.Pitch = -0.40464483460472733;
                    ViewLayer.GlobeCamera.CameraDistance = 6378.3383225536618;
                    break;

                case "mahakaltemple":
                    ViewLayer.GlobeCamera.Yaw = 1.3223640866377953;
                    ViewLayer.GlobeCamera.Pitch = -0.40466608087356637;
                    ViewLayer.GlobeCamera.CameraDistance = 6378.9183152442511;
                    break;
                case "vkfeststage":
                    ViewLayer.GlobeCamera.Yaw = 1.3223640866377953;
                    ViewLayer.GlobeCamera.Pitch = -0.40466608087356637;
                    ViewLayer.GlobeCamera.CameraDistance = 6378.9183152442511;
                    break;
            }
        }

	    private int counter = 0;
	    private int preSnapNumber = -1;
        protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {
			//tiles.Update(gameTime);
			//geoGrid.UpdateGrids(ViewLayer.GlobeCamera.CameraDistance);

			//if (Game.Keyboard.IsKeyDown(Keys.Space))
			//	tiles.IsVisible = false;
			//else
			//	tiles.IsVisible = true;

			//Console.WriteLine();
			//
			//if (Game.Keyboard.IsKeyDown(Keys.NumPad4)) temple.LonLatPosition = temple.LonLatPosition - new DVector2(0.000001, 0);
			//if (Game.Keyboard.IsKeyDown(Keys.NumPad5)) temple.LonLatPosition = temple.LonLatPosition - new DVector2(0, 0.000001);
			//if (Game.Keyboard.IsKeyDown(Keys.NumPad6)) temple.LonLatPosition = temple.LonLatPosition + new DVector2(0.000001, 0);
			//if (Game.Keyboard.IsKeyDown(Keys.NumPad8)) temple.LonLatPosition = temple.LonLatPosition + new DVector2(0, 0.000001);
			//
			//Console.WriteLine(temple.LonLatPosition);
			//
			//Console.WriteLine();


			//if (Game.Keyboard.IsKeyDown(Keys.P))
			//	ViewLayer.GlobeCamera.LoadAnimation();

			ViewLayer.GlobeCamera.PlayAnimation(gameTime);

			if (Game.Keyboard.IsKeyDown(Keys.LeftShift))	ViewLayer.GlobeCamera.ToggleViewToPointCamera();
			if (Game.Keyboard.IsKeyDown(Keys.LeftControl))	ViewLayer.GlobeCamera.ToggleFreeSurfaceCamera();
			if (Game.Keyboard.IsKeyDown(Keys.RightShift))	ViewLayer.GlobeCamera.ToggleTopDownCamera();


			var hc = Control as PulseMicroModelControl;

	        //if (hc.CurrentSnapShot.Number == preSnapNumber) return null;
	        //preSnapNumber = hc.CurrentSnapShot.Number;
			
			var agents = hc.GetAgents();
            {
				//TODO this is kostyl

				var zMat = Matrix.RotationZ(MathUtil.PiOverTwo);
	            //counter++;
                //writer.Write(DateTime.Now.Ticks);
				//writer.Write(agents.Count);

				for (int i = 0; i < agents.Count; i++) {
                    var earthGround = levelGrounds[agents[i].Level];
                    var tris		= earthGround.Meshes[0].Triangles;
                    var agent		= agents[i] as ISfAgent;
					var pos			= new Vector3((float)agent.X, (float)agent.Y, 0);

					pos.Z = GetHitHeight(pos, earthPoints[agents[i].Level], tris);

					//writer.Write(agent.Id);
                    //writer.Write(pos.X);
					//writer.Write(pos.Y);
					//writer.Write(pos.Z);
					//writer.Write(agent.Angle);

					_agentLayer.InstancedDataCPU[i] = new ModelLayer.InstancedDataStruct( Matrix.RotationZ(MathUtil.DegreesToRadians(agent.Angle) + MathUtil.PiOverTwo)*Matrix.Translation(pos)*zMat, Color.White );
				}
                _agentLayer.InstancedCountToDraw = agents.Count;

            }


			//bannersLayer.Clear();
            //foreach (var banner in banners) {
			//	banner.Update(ViewLayer.GlobeCamera, bannersLayer);
			//}

			return null;
        }


        protected override void UnloadView()
        {
	        //writer.Seek(0, SeekOrigin.Begin);
			//writer.Write(counter);
			//
            //Console.WriteLine("Writing to file started, total bytes: " + memBuf.Capacity);
			//Console.WriteLine("Number of snapshots: " + counter);
			//using (var stream = File.OpenWrite("snapshots.bin")) {
		    //    memBuf.WriteTo(stream);
	        //}
			//
			//memBuf.Dispose();
        }


		Vector3[] GetMeshPointsTransformed(IList<MeshVertex> meshPoints, Matrix transform) 
		{
			var points = meshPoints.Select(x => Vector3.TransformCoordinate(x.Position, transform)).ToArray();

			for (int i = 0; i < points.Length; i++) {
				var p = points[i];

				var z	= p.Z;
				p.Z		= p.Y;
				p.Y		= -z;

				points[i] = p;
			}

			return points;
		}


		float GetHitHeight(Vector3 pos, Vector3[] points, IList<MeshTriangle> tris)
		{
			float h = pos.Z;

			pos.Z = 10;

			var ray = new Ray(pos, -Vector3.UnitZ);

			foreach (var tri in tris) {
				Vector3 hitPoint;
				var p0 = points[tri.Index0];
				var p1 = points[tri.Index1];
				var p2 = points[tri.Index2];
				if (ray.Intersects(ref p0, ref p1, ref p2, out hitPoint)) {
					h = hitPoint.Z;
					break;
				}
			}

			return h;
		}

	}
}

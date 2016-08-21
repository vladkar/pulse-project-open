using System;
using System.Collections.Generic;
using System.Linq;
using City.Models;
using City.Panel;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Infection;

namespace City.ControlsClient.DomainClient.Train
{
	public class TrainModelView : AbstractPulseView<TrainControl>
	{
		ControlInfo config;

		ModelLayer trainModel;
		ModelLayer railModel;
		ModelLayer agentLayer;
		private ModelLayer platform;
		private float platformMaxDistance = 400000;
		private float platformMinDistance = -400000;
		private float platformVelocity = 1800;
		private float railLength = 1000;

		LandscapeGenerator generator;


		DVector2 targetCoords;
		DVector2 currentCoords;

		private DateTime prevTime;

		private bool isUIInitialized;

		readonly DVector2[] wagonCoords = new DVector2[] {

				//new DVector2(0, 0.0223806466792712),
				//new DVector2(0, 0.0082534591213966),
				new DVector2(0, 0.0152534591213966),
				new DVector2(0, -0.0108760684934075),
				new DVector2(0, -0.0331015937303125),
				new DVector2(0, -0.0561262798863397),
				new DVector2(0, -0.0788013557192525),
				new DVector2(0, -0.10097797650605),
				new DVector2(0, -0.124452090722041),
				new DVector2(0, -0.146477996396482),
				new DVector2(0, -0.169053028065173),
				new DVector2(0, -0.19122865440867),
				new DVector2(0, -0.213354382957863),
				new DVector2(0, -0.236329048861149),
				new DVector2(0, -0.258404832657271),
				new DVector2(0, -0.280830531855194),
				new DVector2(0, -0.303655398051163),

			};


		private TimeSpan trainArrivalPeriod		= TimeSpan.FromMinutes(3);
		private TimeSpan landscapeHoldPeriod	= TimeSpan.FromSeconds(30);


		private Point previousMousePosition;



		public TrainModelView(TrainControl trainControl)
		{
			Control = trainControl;
		}


		public override Frame AddControlsToUI()
		{
			return null;
		}


		protected override void InitializeView()
		{
			///////////////////////// Initialize Train /////////////////////////////////////
			trainModel = new ModelLayer(Game, new DVector2(0, 0), "Train/train-new") {
				ZOrder		= 1500,
				ScaleFactor = 1,
				XRay		= true ,
			};

			railModel = new ModelLayer(Game, new DVector2(0, 0), "Train/rails-very-light", 200) {
				ZOrder		= 1500,
				ScaleFactor = 1,
				XRay		= false,
			};

			for (int i = 0; i < railModel.InstancedDataCPU.Length; i++) {
				railModel.InstancedDataCPU[i].World = Matrix.Translation(railLength * (i - 100), 0, 0);
				railModel.InstancedDataCPU[i].Color = Color.White;
			}


			Layers.Add(new GisLayerWrapper(trainModel));
			Layers.Add(new GisLayerWrapper(railModel));

			

			var tree01 = new ModelLayer(Game, new DVector2(), "Train/tree-05 (2)",	25) {
				ZOrder		= 500,
				ScaleFactor = 1,
				XRay		= true,
			};
			var tree02 = new ModelLayer(Game, new DVector2(), "Train/tree-06 (2)",	25) {
				ZOrder		= 500,
				ScaleFactor = 1,
				XRay		= true,
			};
			//var stone01 = new ModelLayer(Game, new DVector2(), "Train/stones-01",	25) {
			//	ZOrder		= 500,
			//	ScaleFactor = 1,
			//	XRay		= true,
			//};
			//var tree03 = new ModelLayer(Game, new DVector2(), "Train/stones-02",	25) {
			//	ZOrder		= 500,
			//	ScaleFactor = 1,
			//	XRay		= true,
			//};
			var tree04 = new ModelLayer(Game, new DVector2(), "Train/tree-08",	25) {
				ZOrder		= 500,
				ScaleFactor = 1,
				XRay		= true,
			};
			var tree05 = new ModelLayer(Game, new DVector2(), "Train/tree-09",	25) {
				ZOrder		= 500,
				ScaleFactor = 1,
				XRay		= true,
			};


			Layers.Add(new GisLayerWrapper(tree01));
			Layers.Add(new GisLayerWrapper(tree02));
			//Layers.Add(new GisLayerWrapper(stone01));
			//Layers.Add(new GisLayerWrapper(tree03));
			Layers.Add(new GisLayerWrapper(tree04));
			Layers.Add(new GisLayerWrapper(tree05));

			var models = new List<ModelLayer>() { tree01, tree02, tree04, tree05 };

			generator = new LandscapeGenerator(models);
			generator.MinDist = platformMinDistance;
			generator.MaxDist = platformMaxDistance;


			///////////////////////// Initialize Agents Model /////////////////////////////////////
			agentLayer = new ModelLayer(Game, new DVector2(0,0), "human_agent", 5000) {
				ScaleFactor = 100,
				ZOrder		= 1501,
				XRay		= false,
				IsVisible	= true
			};

			Layers.Add(new GisLayerWrapper(agentLayer));


			platform = new ModelLayer(Game, new DVector2(0, 0), "Train/platform.fbx", 1) {
				ZOrder		= 1500,
				ScaleFactor = 1,
				XRay		= true,
			};
			Layers.Add(new GisLayerWrapper(platform));

			platform.InstancedDataCPU[0].Color = Color.White;

			trainArrivalPeriod = TimeSpan.FromSeconds(platformMaxDistance / platformVelocity);

			generator.Velocity = platformVelocity;

			isUIInitialized = false;
		}


		protected override void LoadView(ControlInfo controlInfo)
		{
			if (!isUIInitialized) {
				var ui = ((CustomGameInterface)Game.GameInterface).ui;
				var frame = (Panel as GisPanel).Frame;
				var WagonButton = CreateTrainControls(ui, 14);
				WagonButton.X = frame.Width / 2 - WagonButton.Width / 2;
				WagonButton.Y = frame.Height - WagonButton.Height - ConstantFrameUI.offsetTrainButtonList;
				frame.Add(WagonButton);

				/////////////////////////// Input /////////////////////////
				//Game.Touch.Tap += p => {
				//	var f = ConstantFrameUI.GetHoveredFrame(frame, p.Position);
				//	f?.OnClick(Keys.LeftButton, false);
				//};

				Game.Touch.Manipulate += p => {
					var f = ConstantFrameUI.GetHoveredFrame(frame, p.Position);
					if (f != null) {
						ViewLayer.GlobeCamera.CameraZoom(MathUtil.Clamp(1.0f - p.ScaleDelta, -0.3f, 0.3f));

						if (p.IsEventEnd) return;

						if (p.IsEventBegin)
							previousMousePosition = p.Position;

						ViewLayer.GlobeCamera.RotateViewToPointCamera((Vector2)p.Position - (Vector2)previousMousePosition);

						previousMousePosition = p.Position;
					}
				};

				isUIInitialized = true;
			}

			config = controlInfo;

			///////////////////////// Initialize camera /////////////////////////////////////
			ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 5;
			ViewLayer.GlobeCamera.Yaw	= 0;
			ViewLayer.GlobeCamera.Pitch = 0;

			ViewLayer.GlobeCamera.CameraState = GlobeCamera.CameraStates.ViewToPoint;

			ViewLayer.GlobeCamera.ViewToPointPitch	= -Math.PI / 25.0;
			ViewLayer.GlobeCamera.ViewToPointYaw	= 0.53159265358979;
			ViewLayer.GlobeCamera.CameraDistance	= 6381.265;

			ViewLayer.GlobeCamera.Parameters.MinCameraDistance = GeoHelper.EarthRadius + 2.0;
			ViewLayer.GlobeCamera.Parameters.MaxCameraDistance = GeoHelper.EarthRadius + 20;

			ViewLayer.GlobeCamera.Parameters.MinViewToPointPitch = -Math.PI / 25.0;

			currentCoords	= wagonCoords[0];
			targetCoords	= wagonCoords[0];

			prevTime = Control.CurrentTime;
		}


		private bool setPosition = true;
		protected override ICommandSnapshot UpdateView(GameTime gameTime)
		{
			if (Game.Keyboard.IsKeyDown(Keys.D1))
				trainModel.IsVisible = true;
            if (Game.Keyboard.IsKeyDown(Keys.D2))
				trainModel.IsVisible = false;

			var hc = Control as TrainControl;

			var time	= hc.CurrentTime;
			var stop	= hc.CurrentRoute.GetNearestCity(time);

			var diff = Control.CurrentTime - prevTime;
			if(diff.Ticks < 0 || diff.Days > 365) diff = new TimeSpan(0);

			prevTime = Control.CurrentTime;
			
			// Train in city
			if (time >= stop.ArrivalTime && (time <= stop.DepartureTime || stop == Control.CurrentRoute.Schedule.StopsPlacesList.Last())) {
				platform.InstancedDataCPU[0].World = Matrix.Translation(0,0,0);
				platform.InstancedDataCPU[0].Color.Alpha = 1.0f;
				setPosition = true;
			}
			else {
				platform.InstancedDataCPU[0].Color.Alpha = 0.0f;
				// Train arriving
				if (time < stop.ArrivalTime && time >= stop.ArrivalTime - trainArrivalPeriod) {
					if (setPosition) {
						setPosition = false;
						platform.InstancedDataCPU[0].World = Matrix.Translation(platformMinDistance, 0, 0);
						platform.InstancedDataCPU[0].Color.Alpha = 1.0f;
					}
					var platformPos = platform.InstancedDataCPU[0].World.TranslationVector;
					platformPos.X += platformVelocity*(float)diff.TotalSeconds;

					platform.InstancedDataCPU[0].World.TranslationVector = platformPos;
					platform.InstancedDataCPU[0].Color.Alpha = 1.0f;
				}
				// Train departing
				if (time > stop.DepartureTime && time <= stop.DepartureTime + trainArrivalPeriod) {
					var platformPos = platform.InstancedDataCPU[0].World.TranslationVector;
					platformPos.X += platformVelocity * (float)diff.TotalSeconds;

					platform.InstancedDataCPU[0].World.TranslationVector = platformPos;
					platform.InstancedDataCPU[0].Color.Alpha = 1.0f;
				}

				if (time < stop.ArrivalTime - trainArrivalPeriod + landscapeHoldPeriod && time >= stop.ArrivalTime - trainArrivalPeriod - landscapeHoldPeriod) {
					generator.IsSpawnActive = false;
				}
				else {
					generator.IsSpawnActive = true;
				}

				for (int i = 0; i < railModel.InstancedDataCPU.Length; i++) {
					var trans = railModel.InstancedDataCPU[i].World.TranslationVector;
					trans.X += platformVelocity*(float) diff.TotalSeconds;
					if (trans.X > railLength*100) trans.X -= railLength*200;
				
					railModel.InstancedDataCPU[i].World.TranslationVector = trans;
				}


				generator.Update(diff);
			}


			currentCoords = DVector2.Lerp(currentCoords, targetCoords, gameTime.ElapsedSec);

			var coords = DMathUtil.DegreesToRadians(currentCoords);

			ViewLayer.GlobeCamera.Yaw	= coords.X;
			ViewLayer.GlobeCamera.Pitch = -coords.Y;


            var agents		= hc.GetAgents();
			var infAgents	= hc.GetInfectedAgents();
            {
				//var zMat = Matrix.RotationZ(MathUtil.PiOverTwo);

	            var visibleCount = Math.Min(agents.Count, agentLayer.InstancedDataCPU.Length);

				for (int i = 0; i < visibleCount; i++) {
                    var agent	= agents[i] as ISfAgent;
					var pos		= new Vector3((float)agent.X, (float)agent.Y, 0);

					var infectionStage = BaseInfectionStage.InfectionStates.S;
                    if(i < infAgents.Count) infectionStage = infAgents[i];

					agentLayer.InstancedDataCPU[i] = new ModelLayer.InstancedDataStruct ( Matrix.RotationZ(MathUtil.DegreesToRadians(agent.Angle) + MathUtil.PiOverTwo) * Matrix.Translation(pos), GetInfectedColor(infectionStage));
                }
				agentLayer.InstancedCountToDraw = visibleCount;

            }
            if (_currentCommand != null)
            {
                var tmp = _currentCommand;
                _currentCommand = null;
                return tmp;
            }

            return null;
		}

	    private Color GetInfectedColor(BaseInfectionStage.InfectionStates stage)
	    {
	        switch (stage)
	        {
	            case BaseInfectionStage.InfectionStates.IM:
	                return Color.Blue;
	            case BaseInfectionStage.InfectionStates.S:
	                return Color.White;
	            case BaseInfectionStage.InfectionStates.E:
	                return Color.Yellow;
	            case BaseInfectionStage.InfectionStates.I2:
	                return Color.Red;
	            default:
					return Color.White;
			}
	    }


	    protected override void UnloadView()
		{
			agentLayer.InstancedCountToDraw = 0;
		}


		public void MoveCameraToWagon(int ind)
		{
			if (ind < 0 || ind >= wagonCoords.Length) return;

			targetCoords = wagonCoords[ind];
		}

        Color backColor = new Color(30, 37, 43, 128);
        private List<Frame> wagonButtons = new List<Frame>();
        public List<Frame> listButtonSpeed = new List<Frame>();
	    private CommandSnapshot _currentCommand;

	    private Frame CreateTrainControls(FrameProcessor ui, int countWagon)
        {
            var listButton = new ListBox(ui, 0, 0, 0, 0, "", Color.Zero) {
                Anchor		= FrameAnchor.Bottom,
                IsHoriz		= true
            };

			var textureTrain = Game.Content.Load<DiscTexture>(@"ui\number");
			var trainButton = new ImageButton(ui, 0, 0, ConstantFrameUI.trainButtonWidth, ConstantFrameUI.trainButtonHeight, "", backColor){
                Image = textureTrain,
                ColorImage = Color.White,
                sizeImageX = ConstantFrameUI.mapButtonImageSize,
                sizeImageY = ConstantFrameUI.mapButtonImageSize,
                IsChangeStatus = false
            };
            trainButton.Click += (e, a) => {
                MoveCameraToWagon(0);
                ChooseCurrentFrame(wagonButtons, trainButton);
            };
			listButton.addElement(trainButton);
            wagonButtons.Add(trainButton);
            for (var i = 1; i <= countWagon; i++) {
	            int ind = i;
	            var b = new Frame(ui, 0, 0, ConstantFrameUI.trainButtonWidth, ConstantFrameUI.trainButtonHeight, i.ToString(), backColor)
	            {
                    TextAlignment = Alignment.MiddleCenter,
                    Font = ConstantFrameUI.sfReg15,
                };
			    b.Click += (e, s) => {
			        MoveCameraToWagon(ind);
                    ChooseCurrentFrame(wagonButtons, b);
			    };
                wagonButtons.Add(b);
                listButton.addElement(b);
            }

			listButton.addElement(new Frame(ui, 0, 0, ConstantFrameUI.trainButtonWidth, ConstantFrameUI.trainButtonHeight, "", Color.Zero));


			var textureClock = Game.Content.Load<DiscTexture>(@"ui\speed");

			var clockButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.trainButtonWidth, ConstantFrameUI.trainButtonHeight, "",
			textureClock, textureClock.Width / 2, textureClock.Height / 2, Color.White, () => { });
			(clockButton as Button).DefaultBackColor = backColor;

            listButton.addElement(clockButton);

            var speedArray = new Dictionary<string, int> { { "1x", 10 }, { "5x", 50 }, { "10x", 100 }, { "Max", 0 } };
            foreach (var speed in speedArray) {
	            var b = new Frame(ui, 0, 0, ConstantFrameUI.trainButtonWidth, ConstantFrameUI.trainButtonHeight, speed.Key, backColor)
	            {
                    TextAlignment = Alignment.MiddleCenter,
                    Font = ConstantFrameUI.sfReg15
                };                
                listButtonSpeed.Add(b);
                listButton.addElement(b);
                b.Click += (e, s) => { ChooseCurrentFrame(listButtonSpeed, b); };
                b.Click += (e, s) =>
                {
                    _currentCommand = new CommandSnapshot { Command = "simfps", Args = new[] { speed.Value.ToString() } };
                };
            }
            return listButton;
        }

	    public void ChooseCurrentFrame(List<Frame> allFrame, Frame selectedFrame )
	    {
            allFrame.ForEach(e=>e.BackColor = backColor);
	        selectedFrame.BackColor = ColorConstant.ActiveElement;
	    }
	}
}

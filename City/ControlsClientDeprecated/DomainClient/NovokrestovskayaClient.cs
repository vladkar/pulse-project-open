using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace City.ControlsClient.DomainClient
{
    public class NovokrestovskayaClient : AbstractPulseAgentClient
    {
        private PointsGisLayer _agentsLayer;
        private ModelLayer _subwayAgents;
        private ModelLayer _station;

		public Train LeftTrain;
		public Train RightTrain;

        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            base.LoadLevel(controlInfo);

            _station = new ModelLayer(Game, new DVector2(30.21175, 59.972952), "abstr_station_zero_coords")
            {
                ScaleFactor = 1.0f,
                XRay		= true,
                ZOrder		= 1000
            };

            GisLayers.Add(_station);

            _subwayAgents = new ModelLayer(Game, _station.LonLatPosition, "human_agent", 3000) {
                ZOrder = 1100
            };

			LeftTrain	= new Train(Game, _station.LonLatPosition, "train_right_zerocoord");
			RightTrain	= new Train(Game, _station.LonLatPosition, "train_left_zerocoord");

			LeftTrain.TrainModel.ZOrder		= 500;
			RightTrain.TrainModel.ZOrder	= 500;

			GisLayers.Add(_subwayAgents);
			GisLayers.Add(LeftTrain.TrainModel);
            GisLayers.Add(RightTrain.TrainModel);
		}


	    public void TrainArrive(bool isLeftTrain, float time = 0.5f)
	    {
		    Train tr = RightTrain;
		    if (isLeftTrain) {
			    tr = LeftTrain;
		    }

			tr.ArriveIn(time);
	    }

		public void TrainDepart(bool isLeftTrain, float time = 0.5f)
	    {
			Train tr = RightTrain;
			if (isLeftTrain) {
				tr = LeftTrain;
			}

			tr.ArriveIn(time);
		}


		public class Train
		{
			public AnimatedModelLayer TrainModel;

			public enum TrainMoveDirection {
				Arrive,
				Departure,
				Wait
			}
			public TrainMoveDirection Direction { get; protected set; } = TrainMoveDirection.Wait;

			public float t				= 0.0f;
			public float Transparency	= 1.0f;

			float arrt = 0.0f;

			public float ArriveTime { get; protected set; } = 1.0f;
			public float DepartTime { get; protected set; } = 1.0f;

			public float Time = 0.0f;
			public float TotalAnimTime => ArriveTime + DepartTime;


			public Train (Game engine, DVector2 lonLatPos, string modelName)
			{
				TrainModel = new AnimatedModelLayer(engine, lonLatPos, modelName);
            }


			public void ArriveIn(float arriveTime)
			{
				if (Direction == TrainMoveDirection.Arrive) return;

				ArriveTime	= arriveTime;
				Time		= 0;
				t			= 0;
				arrt		= 0;
				Direction	= TrainMoveDirection.Arrive;
			}

			public void DepartIn(float departureTime)
			{
				if (Direction == TrainMoveDirection.Departure) return;

				DepartTime	= departureTime;
				//Time		= 0;
				Direction	= TrainMoveDirection.Departure;
			}


			public void Update(GameTime time)
			{
				float delta = time.ElapsedSec;
				if (Direction != TrainMoveDirection.Wait)
					Time += delta;

				Transparency = 1.0f;
				switch (Direction) {
					case TrainMoveDirection.Arrive : {
							arrt += delta / ArriveTime;
							arrt = MathUtil.Clamp(arrt, 0.0f, 1.0f);

							t = arrt * 0.5f;
							if (Time >= ArriveTime) Direction = TrainMoveDirection.Wait;

							break;
						}

					case TrainMoveDirection.Departure: {
							t += delta / DepartTime;

							t = MathUtil.Clamp(t, 0.0f, 1.0f);
							if (Time >= TotalAnimTime) Direction = TrainMoveDirection.Wait;
							break;
						}
				}


				if(Direction != TrainMoveDirection.Wait)
					TrainModel.UpdateAnimation(t);

				if (t < 0.15f) {
					Transparency = t / 0.15f;
				}
				if (t > 0.95f) {
					Transparency = 1.0f - (t - 0.85f) / 0.15f;
				}
				TrainModel.Transparency = Transparency;
			}
		}

        protected override bool Validate()
        {
            if (CurrentSnapShot != null) return true;
            return false;
        }


		protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            LeftTrain.Update(gameTime);
			RightTrain.Update(gameTime);
				

            //		    var platforms = (CurrentSnapShot as PulseSnapshot).Platforms;
		    var platforms = (CurrentSnapShot.Extensions[0] as SubwayStationSnapshotExtension).Platforms;

            if (platforms[0] == 1)
                TrainArrive(true, 0.5f);
            if (platforms[1] == 1)
                TrainArrive(false, 0.5f);
            if (platforms[0] == 0)
                TrainDepart(true, 0.5f);
            if (platforms[1] == 0)
                TrainDepart(false, 0.5f);

			var rot = Matrix.RotationZ(MathUtil.PiOverTwo);
			var s = CurrentSnapShot as PulseSnapshot;
            if (s != null)
            {
                for (int i = 0; i < s.Agents.Count; i++) {
                    var pos = new Vector3((float)s.Agents[i].X, (float)s.Agents[i].Y, 0);
				
                    if (pos.X > 22.0f && pos.X < 88.0f) {
					
                        float f = (pos.X - 22.0f) / (66.0f);
					
                        pos.Z = MathUtil.Lerp(0.0f, -37.7f, f);
                    }
					
                    if (pos.X > 88.0f) pos.Z = -37.7f;

                    _subwayAgents.InstancedDataCPU[i] = new ModelLayer.InstancedDataStruct {
                        World = rot*Matrix.Translation(pos)
                    };
                }
                _subwayAgents.InstancedCountToDraw = s.Agents.Count;
            }
			

			if (Game.Keyboard.IsKeyUp(Keys.I)) {
				isPressed = false;
			}

//			if (GameEngine.Keyboard.IsKeyDown(Keys.I) && !isPressed) {
//				MasterLayer.GlobeCamera.SaveCurrentStateToFile();
//				isPressed = true;
//			}
//
//			if (GameEngine.Keyboard.IsKeyDown(Keys.L)) {
//				MasterLayer.GlobeCamera.LoadAnimation();
//				MasterLayer.GlobeCamera.ResetAnimation();
//			}
//			if (GameEngine.Keyboard.IsKeyDown(Keys.K)) {
//				MasterLayer.GlobeCamera.StopAnimation();
//			}
//
//			MasterLayer.GlobeCamera.PlayAnimation(gameTime);
//
//
//			UpdateGrids();

			return null;
        }

		bool isPressed = false;



		public override string UserInfo()
        {
            return "";
        }
    }
}

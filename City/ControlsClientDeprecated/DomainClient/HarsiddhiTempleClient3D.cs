using System;
using System.Collections.Generic;
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
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;
using Fusion.Engine.Graphics;

namespace City.ControlsClient.DomainClient
{
    public class HarsiddhiTempleClient3D : AbstractPulseAgentClient
    {
       // private PolyGisLayer _itmoBilds;

        private ModelLayer _mapAgents;
		private ModelLayer temple;
		Scene			earthGround;
		List<Vector3>	points;


		//TODO extract UI logic to base class
		private PulseAgentUI _aui;
        private ICommandSnapshot _currentCommand = null;

        
        public override Frame AddControlsToUI()
        {
            var gi = Game.GameInterface as CustomGameInterface;
            if (gi == null) return null;
            var ui = gi.ui;


            _aui = new PulseAgentUI();

            _aui.PropertyChanged += (sender, args) =>
            {
                _currentCommand = GetCommand(_aui, args.PropertyName);
            };

            var controlElements = Generator.getControlElement(_aui, ui);
            return controlElements;
        }

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            //base.LoadLevel(controlInfo);

            //TODO
//            _krestMap = PolyGisLayer.CreateFromUtmFbxModel(Game, "HARSIDDHI.FBX");
//            _krestMap.Flags = (int)(PolyGisLayer.PolyFlags.VERTEX_SHADER | PolyGisLayer.PolyFlags.PIXEL_SHADER | PolyGisLayer.PolyFlags.XRAY);
//            _krestMap.IsVisible = true;


//			GisLayers.Add(_krestMap);


            _mapAgents = new ModelLayer(Game, new DVector2(30.21175, 59.972952), "human_agent", 10000)
            {
                ZOrder = 1500,
                ScaleFactor = 1
            };
            _mapAgents.IsVisible = true;

			temple = new ModelLayer(Game, new DVector2(30.21175, 59.972952), "India/harsiddhi_v02")
			{
				ZOrder = 1500,
				ScaleFactor = 1,
				XRay = true
			};

			GisLayers.Add(_mapAgents);
			GisLayers.Add(temple);


			earthGround = Game.Content.Load<Scene>("India/earth_ground");


			var transforms = new Matrix[earthGround.Nodes.Count];
			earthGround.ComputeAbsoluteTransforms(transforms);

			points = earthGround.Meshes[0].Vertices.Select(x=> Vector3.TransformCoordinate(x.Position, transforms[1]) ).ToList();

			for(int i = 0; i < points.Count; i++) {
				var p = points[i];

				var z = p.Z;
				p.Z = p.Y;
				p.Y = -z;

				points[i] = p;
			}
		}

        
        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            var s = CurrentSnapShot as PulseSnapshot;
            if (s != null)
            {
                var rot = Matrix.RotationZ(MathUtil.PiOverTwo);

				var tris = earthGround.Meshes[0].Triangles;


				for (int i = 0; i < s.Agents.Count; i++) {
					var pos = new Vector3((float)s.Agents[i].X, (float)s.Agents[i].Y, 100);

					var ray = new Ray(pos, -Vector3.UnitZ);

					
					foreach (var tri in tris)
					{
						Vector3 hitPoint;
						var p0 = points[tri.Index0];
						var p1 = points[tri.Index1];
						var p2 = points[tri.Index2];
                        if (ray.Intersects(ref p0, ref p1, ref p2, out hitPoint)) {
							pos.Z = hitPoint.Z;
							break;
                        }
					}

					_mapAgents.InstancedDataCPU[i] = new ModelLayer.InstancedDataStruct {
						World = rot * Matrix.Translation(pos)
					};
				}
                _mapAgents.InstancedCountToDraw = s.Agents.Count;
                
            }

            

            if (_currentCommand != null)
            {
                var tmp = _currentCommand;
                _currentCommand = null;
                return tmp;
            }

            return null;
        }

        private ICommandSnapshot GetCommand(PulseAgentUI aui, string propertyName)
        {
            switch (propertyName)
            {
                case "flow":
                    return new CommandSnapshot {Command = propertyName, Args = new[] { aui.Slider.ToString()}};
                case "sf":
                    return new CommandSnapshot
                    {
                        Command = propertyName,
                        Args = new[]
                        {
                            aui.RepulsiveAgentField,
                            aui.RepulsiveAgentFactorField,
                            aui.RepulsiveObstacleField,
                            aui.RepulsiveObstacleFactorField
                        }
                    };
                default:
                    return null;
            }
        }

        public override string UserInfo()
        {
            return "";
        }
    }
}

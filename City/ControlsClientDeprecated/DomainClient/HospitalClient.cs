using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace City.ControlsClient.DomainClient
{
    public class HospitalClient : AbstractPulseAgentClient
    {
        private ModelLayer _agents;
        private ModelLayer _hospital;

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

            _hospital = new ModelLayer(Game, new DVector2(30.22175, 59.972952), "almaz_hospital")
            {
                ScaleFactor = 1.0f,
                XRay = true,
                ZOrder = 1000
            };

            GisLayers.Add(_hospital);

            _agents = new ModelLayer(Game, _hospital.LonLatPosition, "human_agent", 3000)
            {
                ZOrder = 1100
            };

            
            GisLayers.Add(_agents);
		}

		public override ICommandSnapshot Update(GameTime gameTime)
        {
			var rot = Matrix.RotationZ(MathUtil.PiOverTwo);
			var s = CurrentSnapShot as PulseSnapshot;
                if (s != null)
                {
                    for (int i = 0; i < s.Agents.Count; i++) {
                        var pos = new Vector3((float)s.Agents[i].X - 86, (float)s.Agents[i].Y + 30, 11);
//					
//                        if (pos.X > 22.0f && pos.X < 88.0f) {
//						
//                            float f = (pos.X - 22.0f) / (66.0f);
//						
//                            pos.Z = MathUtil.Lerp(0.0f, -37.7f, f);
//                        }
//						
//                        if (pos.X > 88.0f) pos.Z = -37.7f;
//
                        _agents.InstancedDataCPU[i] = new ModelLayer.InstancedDataStruct {
                            World = Matrix.Translation(pos)
                        };
                    }
                    _agents.InstancedCountToDraw = s.Agents.Count;
                }

			return null;
        }
        
		public override string UserInfo()
        {
            return "";
        }
    }
}

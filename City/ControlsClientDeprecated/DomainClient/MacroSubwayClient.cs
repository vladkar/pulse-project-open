using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using City.Snapshot;
using City.Snapshot.Snapshot;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Frames;

namespace City.ControlsClient.DomainClient
{
    public class MacroSubwayClient : AbstractPulseClient
    {
        public List<SubwayStation> SubwayStations { get; set; }
        private PointsGisLayer globePosts;
        private LinesGisLayer subwayLines;
        private ModelLayer subway3DStationsInput;
        private ModelLayer subway3DStationsOutput;
        private ModelLayer.InstancedDataStruct[] matrixInput; 
        private ModelLayer.InstancedDataStruct[] matrixOutput; 

        int timeIndex = 0;
        bool Pause;
        List<DateTime> dates = new List<DateTime>();
        float tt = 0;
        float speed = 0.25f;
        Color inColor = new Color(212, 68, 40);
        Color outColor = new Color(70, 107, 166);

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
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            var str = File.ReadAllLines(dir + @"SaintPetersburg\subway_in_dataset.csv")[0];
            var times = str.Split(';').Skip(3).ToList();
            times.ForEach((x) => dates.Add(DateTime.Parse(x)));

            var stations = File.ReadAllLines(dir + @"SaintPetersburg\subway_in_dataset.csv").Skip(1).Select(a =>
            {
                var rawPost = a.Split(';');
                List<int> inflow = new List<int>();
                for(int i = 3; i < rawPost.Length; i++)
                {
                    inflow.Add((int)Double.Parse(rawPost[i]));
                }
                return new SubwayStation
                {
                    Name = rawPost[0],
                    Lat = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
                    Lon = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
                    InFlow = inflow
                };
            }).ToList();

            int counter = 0;
            var outflowReader = File.ReadAllLines(dir + @"SaintPetersburg\subway_out_dataset.csv").Skip(1);
            foreach(var o in outflowReader)
            {
                var rawPost = o.Split(';');
                List<int> outflow = new List<int>();
                for (int i = 3; i < rawPost.Length; i++)
                {
                    outflow.Add((int)Double.Parse(rawPost[i]));
                }
                stations[counter].OutFlow = outflow;
                counter++;
            }

            stations.RemoveAll((x) => x.Lat == 0);
            SubwayStations = stations;

            

            globePosts = new PointsGisLayer(Game, SubwayStations.Count * 2)
            {
                ImageSizeInAtlas = new Vector2(36, 36),
                TextureAtlas = Game.Content.Load<Texture2D>("circles.tga")
            };

            for (int i = 0; i < SubwayStations.Count; i++)
            {
                var station = SubwayStations[i];                
                globePosts.PointsCpu[2 * i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(station.Lon),
                    Lat = DMathUtil.DegreesToRadians(station.Lat),
                    Color = inColor,
                    Tex0 = new Vector4(12, 0, 0.4f, 0.0f)
                };
                globePosts.PointsCpu[2 * i + 1] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(station.Lon),
                    Lat = DMathUtil.DegreesToRadians(station.Lat),
                    Color = outColor,
                    Tex0 = new Vector4(13, 0, 0.4f, 0.0f)
                };
            }
            globePosts.UpdatePointsBuffer();


            //add lines
            var lineReader = File.ReadAllLines(dir + @"SaintPetersburg\subway_lines_coord.txt");
            subwayLines = new LinesGisLayer(Game, lineReader.Length * 2);
            subwayLines.Flags = (int)(LinesGisLayer.LineFlags.DRAW_LINES | LinesGisLayer.LineFlags.ADD_CAPS);

            int count = 0;
            foreach(var l in lineReader)
            {               
                var rawPost = l.Split(';');
                List<int> inflow = new List<int>();
                
                subwayLines.PointsCpu[2 * count] =  new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(Double.Parse(rawPost[0], CultureInfo.InvariantCulture)),
                    Lat = DMathUtil.DegreesToRadians(Double.Parse(rawPost[1], CultureInfo.InvariantCulture)),
                    Color = new Color4(1.0f, 1.0f, 1.0f, 0.25f),
                    Tex0 = new Vector4(0.04f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                };
                subwayLines.PointsCpu[2 * count + 1]  = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(Double.Parse(rawPost[2], CultureInfo.InvariantCulture)),
                    Lat = DMathUtil.DegreesToRadians(Double.Parse(rawPost[3], CultureInfo.InvariantCulture)),
                    Color = new Color4(1.0f, 1.0f, 1.0f, 0.25f),
                    Tex0 = new Vector4(0.04f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                };
                count++;
            };
            subwayLines.UpdatePointsBuffer();
            GisLayers.Add(subwayLines);

           // MasterLayer.GisLayers.Add(globePosts);
            Log.Message("" + globePosts.PointsCount);

            //add 3D stations
            matrixInput = new ModelLayer.InstancedDataStruct[SubwayStations.Count];
            matrixOutput = new ModelLayer.InstancedDataStruct[SubwayStations.Count];
            var globCam = new GlobeCamera(null);
            var startPoint = GeoHelper.SphericalToCartesian(new DVector2(DMathUtil.DegreesToRadians(stations[0].Lon), DMathUtil.DegreesToRadians(stations[0].Lat)), globCam.EarthRadius);


            var normal = DVector3.Normalize(startPoint);

            var xAxis = DVector3.Transform(DVector3.UnitX, DQuaternion.RotationAxis(DVector3.UnitY, DMathUtil.DegreesToRadians(stations[0].Lon)));
            xAxis.Normalize();

            var zAxis = DVector3.Cross(xAxis, normal);
            zAxis.Normalize();

            Matrix rotationMat = Matrix.Identity;
            rotationMat.Forward = xAxis.ToVector3();
            rotationMat.Up = normal.ToVector3();
            rotationMat.Right = zAxis.ToVector3();
            rotationMat.TranslationVector = Vector3.Zero;
            rotationMat.Invert();

            for (int i = 0; i < SubwayStations.Count; i++) {
                var point = GeoHelper.SphericalToCartesian(new DVector2(DMathUtil.DegreesToRadians(stations[i].Lon), DMathUtil.DegreesToRadians(stations[i].Lat)), globCam.EarthRadius);

                var dif = (point - startPoint) * 1000;
                var len = dif.Length();

                var diff = new Vector3((float)dif.X, (float)dif.Y, (float)dif.Z);
                diff = Vector3.TransformCoordinate(diff, rotationMat);

                matrixInput[i].World = Matrix.RotationYawPitchRoll(0, 0, MathUtil.PiOverTwo) * Matrix.Scaling(30.0f) * Matrix.Translation(new Vector3(diff.X, -diff.Z, diff.Y));
                matrixOutput[i].World = Matrix.RotationYawPitchRoll(0, 0, -MathUtil.PiOverTwo) * Matrix.Scaling(30.0f) * Matrix.Translation(new Vector3(diff.X, -diff.Z, diff.Y));
            }

            subway3DStationsInput = new ModelLayer(Game, new DVector2(stations[0].Lon, stations[0].Lat), "cylinder", SubwayStations.Count)
            {
                XRay = true,
                Yaw = 0,
                ScaleFactor = 1.0f,
                OverallColor = inColor,
                UseOverallColor = true,
            };
            Array.Copy(matrixInput, subway3DStationsInput.InstancedDataCPU, matrixInput.Length);
            subway3DStationsInput.InstancedCountToDraw = SubwayStations.Count;
            GisLayers.Add(subway3DStationsInput);

            subway3DStationsOutput = new ModelLayer(Game, new DVector2(stations[0].Lon, stations[0].Lat), "cylinder", SubwayStations.Count)
            {
                XRay = true,
                Yaw = 0,
                ScaleFactor = 1.0f,
                OverallColor = outColor,
                UseOverallColor = true,
            };
            Array.Copy(matrixOutput, subway3DStationsOutput.InstancedDataCPU, matrixOutput.Length);
            subway3DStationsOutput.InstancedCountToDraw = SubwayStations.Count;
            GisLayers.Add(subway3DStationsOutput);

            Pause = false;
        }

        public override void UnloadLevel()
        {
            base.UnloadLevel();
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            tt += gameTime.ElapsedSec;

            if (tt > speed)
            {
                tt = tt - speed;
                if (!Pause)
                {
                    timeIndex++;
                    Log.Message("" + dates.ElementAt(timeIndex));
                }
            }
            UpdateDotSize();
            return null;
        }

        public override string UserInfo()
        {
            return $"s";
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
        }

        private void UpdateDotSize()
        {
            if (timeIndex > dates.Count) return;
            var newMatrixInput = new ModelLayer.InstancedDataStruct[SubwayStations.Count];
            var newMatrixOutput = new ModelLayer.InstancedDataStruct[SubwayStations.Count]; 

            for (int i = 0; i < SubwayStations.Count; i++)
            {
                var station = SubwayStations[i];
                float currentRadiusIn   = sizeCoeff( station.InFlow.ElementAt(timeIndex)        );
                float nextRadiusIn      = sizeCoeff( station.InFlow.ElementAt(timeIndex + 1)    );

                float currentRadiusOut  = sizeCoeff( station.OutFlow.ElementAt(timeIndex)       );
                float nextRadiusOut     = sizeCoeff( station.OutFlow.ElementAt(timeIndex + 1)   );

                globePosts.PointsCpu[2 * i].Tex0.Z      = currentRadiusIn + (nextRadiusIn - currentRadiusIn) * tt / speed ;             
                globePosts.PointsCpu[2 * i + 1].Tex0.Z  = currentRadiusOut + (nextRadiusOut - currentRadiusOut) * tt / speed ;

                float approxIn = currentRadiusIn + (nextRadiusIn - currentRadiusIn)*tt/speed;
                float approxOut = currentRadiusOut + (nextRadiusOut - currentRadiusOut)*tt/speed;

				Vector3 inputScaleVector = new Vector3(approxIn / 0.8f, approxIn / 0.8f, approxIn * 8);
				Vector3 outputScaleVector = new Vector3(approxOut / 0.8f, approxOut / 0.8f, approxOut * 8);

				newMatrixInput[i].World = Matrix.Scaling(inputScaleVector) * matrixInput[i].World;
				newMatrixOutput[i].World = Matrix.Scaling(outputScaleVector) * matrixOutput[i].World;


			}
			globePosts.UpdatePointsBuffer();

            Array.Copy(newMatrixInput, subway3DStationsInput.InstancedDataCPU, newMatrixInput.Length);

            Array.Copy(newMatrixOutput, subway3DStationsOutput.InstancedDataCPU, newMatrixOutput.Length);
        }

        private float sizeCoeff(float d)
        {
            if (d < 50) return 0.2f;
            if (d < 100) return 0.4f;
            if (d < 500) return 0.5f;
            if (d < 1000) return 0.70f;
            if (d < 2000) return 0.85f;
            return 1.0f;
        }
    }
}
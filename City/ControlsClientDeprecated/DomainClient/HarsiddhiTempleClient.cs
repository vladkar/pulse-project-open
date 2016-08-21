using System;
using System.Collections.Generic;
using System.Linq;
using City.Snapshot;
using City.Snapshot.Navfield;
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

namespace City.ControlsClient.DomainClient
{
    public class HarsiddhiTempleClient : AbstractPulseAgentClient
    {
        private PolyGisLayer _krestMap;
       // private PolyGisLayer _itmoBilds;
        private HeatMapLayer _heatMap;
        private PointsGisLayer _geoAgents;
        private TextGisLayer _textLayer;

        private ModelLayer _mapAgents;
        private LinesGisLayer _obstacleLayer;
        private LinesGisLayer _poiLayer;
        private LinesGisLayer _portalLayer;
        
        private LinesGisLayer _navfieldLayer;


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

            _heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 30.210857, 30.229397, 59.975717, 59.957073, 10, 256, 256, new MercatorProjection());
            _heatMap.MaxHeatMapLevel = 25;
            _heatMap.InterpFactor = 1.0f;
	        _heatMap.IsVisible = true;

//			GisLayers.Add(_krestMap);
            GisLayers.Add(_heatMap);

            _geoAgents = new PointsGisLayer(Game, 10000, false)
            {
                ImageSizeInAtlas	= new Vector2(36, 36),
                TextureAtlas		= Game.Content.Load<Texture2D>("circles.tga"),
                SizeMultiplier = 0.3f,
                ZOrder = 1600
            };

            _mapAgents = new ModelLayer(Game, new DVector2(30.21175, 59.972952), "human_agent", 10000)
            {
                ZOrder = 1500,
                ScaleFactor = 30
            };

            _textLayer = new TextGisLayer(Game, 10000, null);
            _textLayer.ZOrder = 1600;
//            _textLayer.Scale = 100;
            _textLayer.IsVisible = true;
			_textLayer.MaxZoom = 6378.5978289702416;
			_textLayer.MinZoom = 6378.2349263436372;

			GisLayers.Add(_textLayer);

            GisLayers.Add(_geoAgents);
            GisLayers.Add(_mapAgents);


            //lines

            var geoutil = new GeoCartesUtil(PulseMap.MapInfo.MinGeo, PulseMap.MapInfo.MetersPerMapUnit);

            //obstacles
            _obstacleLayer = new LinesGisLayer(Game, 10000);
            _obstacleLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _obstacleLayer.ZOrder = 1000;
            GisLayers.Add(_obstacleLayer);
            

            var obstacles = PulseMap.Levels.Values.First().Obstacles;
            var obstLines = obstacles.SelectMany(o =>
            {
                var t = new List<PulseVector2>();
                for (int i = 0; i < o.Length - 1; i++)
                {
                    t.Add(o[i]);
                    t.Add(o[i + 1]);
                }


                return t;
            }).ToArray();

            for (int i = 0; i < obstLines.Length; i += 1)
            {
                var geoPoint = geoutil.GetGeoCoords(obstLines[i]);
                _obstacleLayer.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                    Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                    Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
            }
            _obstacleLayer.UpdatePointsBuffer();
            _obstacleLayer.IsVisible = true;

            //pois
            _poiLayer = new LinesGisLayer(Game, 1000);
            _poiLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _poiLayer.ZOrder = 900;
            GisLayers.Add(_poiLayer);

            var pois = PulseMap.Levels.Values.First().PointsOfInterest;
            var poiLines = pois.SelectMany(o =>
            {
                var t = new List<PulseVector2>();
                for (int i = 0; i < o.Polygon.Length - 1; i++)
                {
                    t.Add(o.Polygon[i]);
                    t.Add(o.Polygon[i + 1]);
                }

                t.Add(o.Polygon.Last());
                t.Add(o.Polygon.First());

                return t;
            }).ToArray();

            for (int i = 0; i < poiLines.Length; i += 1)
            {
                var geoPoint = geoutil.GetGeoCoords(poiLines[i]);
                _poiLayer.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                    Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                    Color = new Color4(1f, 1f, 0.0f, 1.0f),
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
            }

            _poiLayer.UpdatePointsBuffer();
            _poiLayer.IsVisible = true;

            //portals
            _portalLayer = new LinesGisLayer(Game, 1000);
            _portalLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _portalLayer.ZOrder = 1100;
            GisLayers.Add(_portalLayer);

            var portals = PulseMap.Levels.Values.First().ExternalPortals;
            var prtLines = portals.SelectMany(o =>
            {
                var t = new List<PulseVector2>();
                for (int i = 0; i < o.Polygon.Length - 1; i++)
                {
                    t.Add(o.Polygon[i]);
                    t.Add(o.Polygon[i + 1]);
                }

                t.Add(o.Polygon.Last());
                t.Add(o.Polygon.First());

                return t;
            }).ToArray();

            for (int i = 0; i < prtLines.Length; i += 1)
            {
                var geoPoint = geoutil.GetGeoCoords(prtLines[i]);
                _portalLayer.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                    Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                    Color = new Color4(1f, 0.0f, 0.0f, 1.0f),
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
            }
            _portalLayer.UpdatePointsBuffer();
            _portalLayer.IsVisible = true;




            _navfieldLayer = new LinesGisLayer(Game, 100000);
            _navfieldLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _navfieldLayer.IsVisible = true;
            GisLayers.Add(_navfieldLayer);
        }

        private bool _nfFlaf = false;
        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            var s = CurrentSnapShot as PulseSnapshot;
            if (s != null)
            {
				var count = Math.Min(s.Agents.Count, _geoAgents.PointsCpu.Length);

                for (int i = 0; i < count; i++)
                {
                    var point = new DVector2(DMathUtil.DegreesToRadians(s.Agents[i].Y), DMathUtil.DegreesToRadians(s.Agents[i].X));
                    _geoAgents.PointsCpu[i] = new Gis.GeoPoint
                    {
                        Lon = point.X,
                        Lat = point.Y,
                        Color = Color.White,
                        Tex0 = new Vector4(0, 0, 0.002f, 3.14f)
                    };

                    _textLayer.GeoTextArray[i] = new TextGisLayer.GeoText
                    {
                        Color = Color.Yellow,
                        LonLat = point,
                        Text = s.Agents[i].Id.ToString()
                    };

                   // _heatMap.AddValue(s.Agents[i].Y, s.Agents[i].X, 10.0f);
                }
                _geoAgents.UpdatePointsBuffer();
                _geoAgents.PointsCountToDraw	= count;
				_textLayer.LinesCountToDraw		= count;
                
                _heatMap.UpdateHeatMap();
            }



			if (s.Extensions.Count > 0)
            {
                var nfext = s.Extensions.Values.First() as NavfieldSnapshotExtension;
                if (nfext != null && _nfFlaf == false)
                {
                    if (nfext.Grid.Length > 3)
                        _nfFlaf = true;


                    var geoutil = new GeoCartesUtil(PulseMap.MapInfo.MinGeo, PulseMap.MapInfo.MetersPerMapUnit);

                    var arrows = nfext.Grid;
                    var arrowLine = new List<PulseVector2>();
                    for (int i = 0; i < nfext.Grid.Length; i++)
                    {
                        for (int j = 0; j < nfext.Grid[0].Length; j++)
                        {
                            var gridbl = new PulseVector2(4.62417, 3.8723); 
                            var arrowOffset = new PulseVector2(i*nfext.Size + nfext.Size/2, j*nfext.Size + nfext.Size/2);
                            var arrowStart = gridbl + nfext.BottomLeft + arrowOffset;
                            var arrowEnd = arrowStart + new PulseVector2(0, nfext.Size/2).RotateRadians(nfext.Grid[i][j]);
//                            var arrowEnd = arrowEndN.;

                            arrowLine.Add(arrowStart);
                            arrowLine.Add(arrowEnd);
                        }
                    }


                    for (int i = 0; i < arrowLine.Count; i += 1)
                    {
                        var geoPoint = geoutil.GetGeoCoords(arrowLine[i]);
                        _navfieldLayer.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                            Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                            Color = new Color4(0.0f, 0.0f, 1.0f, 1.0f),
                            Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                        };
                    }

                    _navfieldLayer.IsVisible = true;
                    _navfieldLayer.UpdatePointsBuffer();
                }
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
                case "simfps":
                    return new CommandSnapshot { Command = propertyName, Args = new[] { aui.Fps } };
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

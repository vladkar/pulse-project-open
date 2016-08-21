using System;
using System.Collections.Generic;
using System.Linq;
using City.Models;
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



namespace City.ControlsClient.DomainClient
{
    public class PulseMicroModelDeveloperView : AbstractPulseView<PulseMicroModelControl>
    {
        private PointsGisLayer _geoAgents;
        private TextGisLayer _textLayer;
        private LinesGisLayer _obstacleLayer;
        private LinesGisLayer _poiLayer;
        private LinesGisLayer _portalLayer;
        private LinesGisLayer _fieldLayer;
        private LinesGisLayer _navfieldLayer;
        private LinesGisLayer _forceLayer;

        private LinesGisLayer _levelBorderLayer;
        private TextGisLayer _infoTextLayer;

        private HeatMapLayer _heatMap;

        //TODO extract UI logic to base class
        private PulseAgentUI _aui;
        private ICommandSnapshot _currentCommand = null;


        private const int WIDTH_PLOT = 200;
        private const int HEIGHT_PLOT = 200;

        private ListBox listPlot;
        private GraphicBase plot1;
        private GraphicBase plot2;
        private GraphicBase plot3;
        
        private PulseVector2 _min = new PulseVector2(double.MaxValue, double.MaxValue);
        private PulseVector2 _max = new PulseVector2(double.MinValue, double.MinValue);

        private IDictionary<int, List<DVector2>> polygons = new Dictionary<int, List<DVector2>>(); 


        public PulseMicroModelDeveloperView(PulseMicroModelControl pulseMicroModelControl)
        {
            Control = pulseMicroModelControl;
        }

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

        public void SetPolygons(List<Polygon> polygonList, Frame currentFrame)
        {
            var positionStart = 0;
            
            for (int i=0; i<_fieldLayer.PointsCpu.Length; i++)
            {
                _fieldLayer.PointsCpu[i] = new Gis.GeoPoint();
            }
            if (polygons != null)
            {
                IEnumerable<int> deletedPolygon = polygons.Keys.Except(polygonList.Select(e => e.Id));
                foreach (var polId in deletedPolygon)
                {
                    plot1.removePlot(polId);
                    plot2.removePlot(polId);
//                    plot3.removePlot(polId);
                }
                if (plot1?.getCountPlot() == 0)
                {
                    listPlot.Clear(listPlot);
                    currentFrame.Remove(listPlot);
                    plot1 = null;
                    plot2 = null;
                    plot3 = null;
                }
            }
            polygons = new Dictionary<int, List<DVector2>>();

            foreach (var polygon in polygonList)
            {
                polygons.Add(polygon.Id, polygon.listPoint);
                var color = ColorConstant.BarChartColor[polygon.Id];
                if(polygon.listPoint.Count<=2)
                    continue;
                for (int i = 0; i < polygon.listPoint.Count - 1; i++)
                {
                    _fieldLayer.PointsCpu[2 * i + positionStart] = new Gis.GeoPoint
                    {
                        Lon = polygon.listPoint[i].X,
                        Lat = polygon.listPoint[i].Y,
                        Color = color,
                        Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                    };
                    _fieldLayer.PointsCpu[2 * i + 1 + positionStart] = new Gis.GeoPoint
                    {
                        Lon = polygon.listPoint[i + 1].X,
                        Lat = polygon.listPoint[i + 1].Y,
                        Color = color,
                        Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                    };
                }
                _fieldLayer.PointsCpu[(polygon.listPoint.Count) * 2 + positionStart] = new Gis.GeoPoint
                {
                    Lon = polygon.listPoint[polygon.listPoint.Count - 1].X,
                    Lat = polygon.listPoint[polygon.listPoint.Count - 1].Y,
                    Color = color,
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
                _fieldLayer.PointsCpu[polygon.listPoint.Count * 2 + 1 + positionStart] = new Gis.GeoPoint
                {
                    Lon = polygon.listPoint[0].X,
                    Lat = polygon.listPoint[0].Y,
                    Color = color,
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
                positionStart += polygon.listPoint.Count*2 + 2;
            }

            _fieldLayer.UpdatePointsBuffer();

            var hc = Control as PulseMicroModelControl;
            var map = hc.GetMap();
            var geoutil = new GeoCartesUtil(map.MapInfo.MinGeo, map.MapInfo.MetersPerMapUnit);
            polygons = polygons.ToDictionary(poly => poly.Key, poly =>
            {
                var gc = poly.Value.Select(p => new GeoCoords(DMathUtil.RadiansToDegrees(p.Y), DMathUtil.RadiansToDegrees(p.X)));
                var mpoly = gc.Select(p => geoutil.GetCoordsTuple(p));
                return mpoly.Select(p => new DVector2(p.X, p.Y)).ToList();
            });


            if (plot1==null && polygonList.Count>0)
                CreatePlots(currentFrame);
        }

        private void CreatePlots(Frame currentFrame)
        {
            var gi = Game.GameInterface as CustomGameInterface;
            if (gi == null) return;
            var ui = gi.ui;

            listPlot = new ListBox(ui, 0, currentFrame.Height - HEIGHT_PLOT, 0, 0,"", Color.Zero)
            {
                IsHoriz = true
            };
            
            
            plot1 = FrameHelper.createPlot(ui, 0, 0, WIDTH_PLOT, HEIGHT_PLOT, null, "Avg crowd pressure", "Crowd pressure, dimensionless", "t, s", ColorConstant.defaultConfigPlot);
            plot2 = FrameHelper.createPlot(ui, 0, 0, WIDTH_PLOT, HEIGHT_PLOT, null, "Avg speed", "Speed, m/s", "t, s", ColorConstant.defaultConfigPlot);
            plot3 = FrameHelper.createPlot(ui, 0, 0, WIDTH_PLOT, HEIGHT_PLOT, null, "", "", "", ColorConstant.defaultConfigPlot);

            plot1.setRangePointPlot(500);
            plot2.setRangePointPlot(500);
            plot3.setRangePointPlot(500);

            listPlot.addElement(plot1);
            listPlot.addElement(plot2);
            listPlot.addElement(plot3);
            listPlot.Add(new Header(ui, 0, 0, listPlot.Width, listPlot.Height, "", Color.Zero));

            currentFrame.Add(listPlot);
        }


        protected override void InitializeView()
        {
            _textLayer = new TextGisLayer(Game, 10000, null);
            _textLayer.ZOrder = 1600;
            
            //            _textLayer.Scale = 100;
            _textLayer.IsVisible = false;
            _textLayer.TextSpriteLayer.Visible = _textLayer.IsVisible;
            _textLayer.MaxZoom = 6378.5978289702416;
            _textLayer.MinZoom = 6378.2349263436372;


            _infoTextLayer = new TextGisLayer(Game, 100, null);
            _infoTextLayer.ZOrder = 1600;
//            _infoTextLayer.Scale = 100;
            _infoTextLayer.IsVisible = true;
            _infoTextLayer.MaxZoom = 6378.5978289702416;
            _infoTextLayer.MinZoom = 6378.2349263436372;
        }

        protected override void LoadView(ControlInfo controlInfo)
        {
            ViewLayer.GlobeCamera.Yaw				= 0.52724863273718869;
            ViewLayer.GlobeCamera.Pitch				= -1.0465448182496619;
            ViewLayer.GlobeCamera.CameraDistance	= 6378.3923573642669;

            //            ViewLayer.GlobeCamera.Yaw = 0.527224026069971;
            //            ViewLayer.GlobeCamera.Pitch = -1.04654411416673;
            //            ViewLayer.GlobeCamera.CameraDistance = 6378.3575872918454;

            //            _heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 30.210857, 30.229397, 59.975717, 59.957073, 10, 256, 256, new MercatorProjection());
            //            _heatMap.MaxHeatMapLevel = 25;
            //            _heatMap.InterpFactor = 1.0f;
            //	        _heatMap.IsVisible = true;
            //
            //            GisLayers.Add(_heatMap);

            _heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 30.2084551961212, 30.2097915664348, 59.9629317205249, 59.9623543420761, 10, 128, 128, new MercatorProjection());
            _heatMap.MaxHeatMapLevel = 10;
            _heatMap.InterpFactor = 1.0f;
            _heatMap.IsVisible = true;
            _heatMap.ZOrder = 100;
            Layers.Add(new GisLayerWrapper(_heatMap));

            _textLayer.GlobeCamera = ViewLayer.GlobeCamera;
            Layers.Add(new SpriteLayerWrapper(_textLayer.TextSpriteLayer));
            Layers.Add(new GisLayerWrapper(_textLayer));

            _infoTextLayer.GlobeCamera = ViewLayer.GlobeCamera;
            Layers.Add(new SpriteLayerWrapper(_infoTextLayer.TextSpriteLayer));
            Layers.Add(new GisLayerWrapper(_infoTextLayer));

            var hc = Control as PulseMicroModelControl;
            var map = hc.GetMap();
            var geoutil = new GeoCartesUtil(map.MapInfo.MinGeo, map.MapInfo.MetersPerMapUnit);

            // additional polygons
            _fieldLayer = new LinesGisLayer(Game, 100);
            _fieldLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _fieldLayer.ZOrder = 500;
            Layers.Add(new GisLayerWrapper(_fieldLayer));
            _fieldLayer.UpdatePointsBuffer();
            _fieldLayer.IsVisible = true;

            _geoAgents = new PointsGisLayer(Game, 10000, false)
            {
                ImageSizeInAtlas	= new Vector2(36, 36),
                TextureAtlas		= Game.Content.Load<Texture2D>("circles1.tga"),
                SizeMultiplier = 0.15f,
                ZOrder = 1600,
                Flags = (int)(PointsGisLayer.PointFlags.DOTS_WORLDSPACE | PointsGisLayer.PointFlags.ROTATION_ANGLE)

            };

            Layers.Add(new GisLayerWrapper(_geoAgents));

//            var minx = double.MaxValue;
//            var maxx = double.MinValue;
//            var miny = double.MaxValue;
//            var maxy = double.MinValue;
            

            foreach (var level in map.Levels)
            {
                foreach (var o in level.Value.Obstacles)
                {
                    for (int i = 0; i < o.Length - 1; i++)
                    {
                        if (o[i].X < _min.X)
                            _min.X = o[i].X;
                        if (o[i].Y < _min.Y)
                            _min.Y = o[i].Y;
                        if (o[i].X > _max.X)
                            _max.X = o[i].X;
                        if (o[i].Y > _max.Y)
                            _max.Y = o[i].Y;
                    }
                }
            }

            _min.X -= 10;
            _max.X += 10;
            _min.Y -= 10;
            _max.Y += 10;

            //lines
            var obstLines = new List<PulseVector2>();
            var poiLines = new List<PulseVector2>();
            var prtLines = new List<PulseVector2>();
            var lvlBorderLines = new List<PulseVector2>();

            foreach (var level in map.Levels)
            {
                var offset = new PulseVector2(level.Key*(_max.X-_min.X), 0);

                lvlBorderLines.Add(new PulseVector2(_min.X, _min.Y) + offset);
                lvlBorderLines.Add(new PulseVector2(_max.X, _min.Y) + offset);

                lvlBorderLines.Add(new PulseVector2(_min.X, _max.Y) + offset);
                lvlBorderLines.Add(new PulseVector2(_max.X, _max.Y) + offset);

                lvlBorderLines.Add(new PulseVector2(_min.X, _min.Y) + offset);
                lvlBorderLines.Add(new PulseVector2(_min.X, _max.Y) + offset);

                lvlBorderLines.Add(new PulseVector2(_max.X, _max.Y) + offset);
                lvlBorderLines.Add(new PulseVector2(_max.X, _min.Y) + offset);

                _infoTextLayer.GeoTextArray[level.Key] = new TextGisLayer.GeoText
                {
                    Color = Color.Green,
                    LonLat = new DVector2(_min.X + offset.X, _max.Y + 10),
                    Text = $"Level {level.Key}"
                };


                foreach (var o in level.Value.Obstacles)
                {
                    for (int i = 0; i < o.Length - 1; i++)
                    {
                        obstLines.Add(o[i] + offset);
                        obstLines.Add(o[i + 1] + offset);
                    }
                }

                foreach (var poi in level.Value.PointsOfInterest)
                {
                    for (int i = 0; i < poi.Polygon.Length - 1; i++)
                    {
                        poiLines.Add(poi.Polygon[i] + offset);
                        poiLines.Add(poi.Polygon[i + 1] + offset);
                    }
                    poiLines.Add(poi.Polygon.Last() + offset);
                    poiLines.Add(poi.Polygon.First() + offset);
                }

                foreach (var prt in level.Value.ExternalPortals)
                {
                    for (int i = 0; i < prt.Polygon.Length - 1; i++)
                    {
                        prtLines.Add(prt.Polygon[i] + offset);
                        prtLines.Add(prt.Polygon[i + 1] + offset);
                    }
                    prtLines.Add(prt.Polygon.Last() + offset);
                    prtLines.Add(prt.Polygon.First() + offset);
                }
            }
            _infoTextLayer.LinesCountToDraw = map.Levels.Count;

            //lvlborders
            _levelBorderLayer = new LinesGisLayer(Game, 1000);
            _levelBorderLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _levelBorderLayer.ZOrder = 1000;
            Layers.Add(new GisLayerWrapper(_levelBorderLayer));
            for (int i = 0; i < lvlBorderLines.Count; i += 1)
            {
                var geoPoint = geoutil.GetGeoCoords(lvlBorderLines[i]);
                _levelBorderLayer.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                    Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                    Color = new Color4(0.0f, 1f, 0.0f, 1.0f),
                    Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };
            }
            _levelBorderLayer.UpdatePointsBuffer();
            _levelBorderLayer.IsVisible = true;

            //obstacles
            _obstacleLayer = new LinesGisLayer(Game, 10000);
            _obstacleLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _obstacleLayer.ZOrder = 1000;
            Layers.Add(new GisLayerWrapper(_obstacleLayer));
            for (int i = 0; i < obstLines.Count; i += 1)
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
            _poiLayer = new LinesGisLayer(Game, 10000);
            _poiLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _poiLayer.ZOrder = 900;
            Layers.Add(new GisLayerWrapper(_poiLayer));
            for (int i = 0; i < poiLines.Count; i += 1)
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
            Layers.Add(new GisLayerWrapper(_portalLayer));
            for (int i = 0; i < prtLines.Count; i += 1)
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

            //force
            _forceLayer = new LinesGisLayer(Game, 10000);
            _forceLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _forceLayer.ZOrder = 1700;
            Layers.Add(new GisLayerWrapper(_forceLayer));


            //navfields
            _navfieldLayer = new LinesGisLayer(Game, 1000000);
            _navfieldLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
            _navfieldLayer.IsVisible = true;
            Layers.Add(new GisLayerWrapper(_navfieldLayer));
        }

        private bool _nfFlaf = false;

        protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {
            var hc = Control as PulseMicroModelControl;
            var agents = hc.GetAgents();
            var navField = hc.GetNavfield();
            var map = hc.GetMap();
            var geoutil = new GeoCartesUtil(map.MapInfo.MinGeo, map.MapInfo.MetersPerMapUnit);

            var count = Math.Min(agents.Count, _geoAgents.PointsCpu.Length);
            
            var densities = polygons.ToDictionary(p => p.Key, p => 0d);
            var speeds = polygons.ToDictionary(p => p.Key, p => 0d);
            var counts = polygons.ToDictionary(p => p.Key, p => 0);
            //            var countinp = polygons.ToDictionary(p => p.Key, p => 0d);


            var forceLines = new List<PulseVector2>();

            for (int i = 0; i < count; i++)
            {
                var agent = agents[i] as ISfAgent;

                var offset = agent.Level * (_max.X - _min.X);
                var gpoint = geoutil.GetGeoCoords(agent.X + offset, agent.Y);
                var point = new DVector2(DMathUtil.DegreesToRadians(gpoint.Lon), DMathUtil.DegreesToRadians(gpoint.Lat));
                _geoAgents.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = point.X,
                    Lat = point.Y,
                    Color = Color.White,
                    Tex0 = new Vector4(16, 1, 0.002f, MathUtil.DegreesToRadians(agent.Angle) + MathUtil.PiOverTwo)
                };

                _textLayer.GeoTextArray[i] = new TextGisLayer.GeoText
                {
                    Color = Color.Yellow,
                    LonLat = new DVector2(point.X, point.Y),
                    Text = agent.Id.ToString()
                };

                foreach (var polygon in polygons)
                {
                    if (ClipperUtil.IsPointInPolygon(new PulseVector2(agent.X + offset, agent.Y), polygon.Value.Select(p => new PulseVector2(p.X, p.Y)).ToArray()))
                    {
                        densities[polygon.Key] += agent.Pressure;
                        speeds[polygon.Key] += agent.StepDist;
                        counts[polygon.Key] += 1;
                    }
                }

                if (_aui.ShowSocialForce)
                {
                    //                    forceLines.Add(new PulseVector2(agent.X + offset, agent.Y));
                    //                    forceLines.Add(new PulseVector2(agent.ForceX + offset, agent.ForceY));

                    var p = new PulseVector2(agent.X + offset, agent.Y);
                    //                    var f = new PulseVector2(agent.ForceX + offset, agent.ForceY);
                    //                    var d = f - p;
                    //                    var r = d*10 + p;


                    var f = new PulseVector2(agent.ForceX, agent.ForceY);
                    var r = p + f*10;

                    forceLines.Add(new PulseVector2(p.X, p.Y));
                    forceLines.Add(new PulseVector2(r.X, r.Y));
                }
            }

            var modelTimeStep = 0.1;
            foreach (var polygon in polygons)
            {
                if (counts[polygon.Key]> 0)
                {
                    densities[polygon.Key] /= counts[polygon.Key];
                    speeds[polygon.Key] = speeds[polygon.Key]/counts[polygon.Key]/modelTimeStep;
                }


                if (plot1 != null && plot2 != null)
                {
                    var point1 = new Vector2((float)gameTime.Total.TotalSeconds, (float) densities[polygon.Key]);
                    plot1.addPointToPlot(point1, polygon.Key);

                    var point2 = new Vector2((float)gameTime.Total.TotalSeconds, (float)speeds[polygon.Key]);
                    plot2.addPointToPlot(point2, polygon.Key);
                }
            }
            

            _geoAgents.UpdatePointsBuffer();
            _geoAgents.PointsCountToDraw = count;
            _textLayer.LinesCountToDraw = count;

            if (_aui.ShowSocialForce)
            {

                for (int i = 0; i < forceLines.Count; i += 1)
                {
                    var geoPoint = geoutil.GetGeoCoords(forceLines[i]);
                    _forceLayer.PointsCpu[i] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(geoPoint.Lon),
                        Lat = DMathUtil.DegreesToRadians(geoPoint.Lat),
                        Color = new Color4(0f, 0.0f, 1.0f, 0.7f),
                        Tex0 = new Vector4(0.001f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                    };
                }

                _forceLayer.UpdatePointsBuffer();
            }

            if (navField != null && _nfFlaf == false)
            {
                if (navField.Grid.Length > 3)
                    _nfFlaf = true;
                
                var offset = new PulseVector2(navField.Level * (_max.X - _min.X), 0);
                var arrows = navField.Grid;
                var arrowLine = new List<PulseVector2>();
                for (int i = 0; i < navField.Grid.Length; i++)
                {
                    for (int j = 0; j < navField.Grid[0].Length; j++)
                    {
                        var gridbl = new PulseVector2(4.62417, 3.8723);
                        var arrowOffset = new PulseVector2(i*navField.Size + navField.Size/2,
                            j*navField.Size + navField.Size/2);
                        var arrowStart = gridbl + navField.BottomLeft + arrowOffset + offset;
                        var arrowEnd = arrowStart +
                                       new PulseVector2(0, navField.Size/2).RotateRadians(navField.Grid[i][j]);
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

            // corr heatmap test
            var corrData = (Control as PulseMicroModelControl).CorrData;

            if (corrData != null && corrData.Values.First().Grid[0, 0].Count > 100)
            {
                var initX = 10;
                var initY = 10;

                var offset = new PulseVector2(corrData.Keys.First() * (_max.X - _min.X), 0);

                var arr = corrData.Values.First().Grid;
                int rowLength = arr.GetLength(0);
                int colLength = arr.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < colLength; j++)
                    {
                        //arr[i, j].Add(arr[i, j]);
                        var coef = ComputeCoeff(arr[i, j].TakeLast(100).Select(d => d.Count).ToArray(),
                            arr[initX, initY].TakeLast(100).Select(d => d.Count).ToArray());

                        coef = double.IsNaN(coef) ? 0 : coef;
                        
                        var offsetLevel = 1*(_max.X - _min.X);
                        var srcpoint = corrData.Values.First().GetCoordsByIndex(i, j);
                        var gpoint = geoutil.GetGeoCoords(srcpoint + offset);
                        //var point = new DVector2(DMathUtil.DegreesToRadians(gpoint.Lon),
                         //   DMathUtil.DegreesToRadians(gpoint.Lat));


                        _heatMap.AddValue(gpoint.Lon, gpoint.Lat, (float) coef*600
                            );
                    }

                    //Console.Write(Environment.NewLine + Environment.NewLine + "|");
                }
                _heatMap.UpdateHeatMap();
            }


            if (_currentCommand != null)
            {
                var tmp = _currentCommand;
                _currentCommand = null;
                return tmp;
            }

            return null;
        }

        protected override void UnloadView()
        {
        }

        public double ComputeCoeff(double[] values1, double[] values2)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("values must be the same length");

            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
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
                case "agentid":
                    _textLayer.IsVisible = !_textLayer.IsVisible;
                    _textLayer.TextSpriteLayer.Visible = _textLayer.IsVisible;
                    _textLayer.Update(null);
                    return null;
                case "showsf":
                    _forceLayer.IsVisible = !_textLayer.IsVisible;
                    _forceLayer.Update(null);
                    return null;

                //                case "polygon":
                //                    return new CommandSnapshot
                //                    {
                //                        Command = propertyName,
                //                        Args = new[]
                //                        {
                ////                            aui.Polygon.Id.ToString(),
                ////                            aui.Polygon.listPoint
                //                        }
                //                    };
                default:
                    return null;
            }
        }
    }
}

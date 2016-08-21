using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using City.Snapshot;
using City.Snapshot.Snapshot;
using City.Snapshot.TrafficAgent;
using City.UIFrames;
using City.UIFrames.FrameElement;
using DistributedTraffic;
using DistributedTraffic.Emergency;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using MultiagentEngine.Engine;
using TrControl;

namespace City.ControlsClient.DomainClient
{
    public class TrafficClient : AbstractPulseClient
    {
//        public Game Game { get; protected set; }


        private SpriteLayer _testLayer;
//        private IDictionary<string, DiscTexture> _textures;
//        private RenderLayer _masterLayer;

        #region pulse fields

        private TrafficEngineFactory _engineFactory;
        private TrafficConfig _config;
        private SimulationEngine _engine;

        private ZonedTrafficMap _map;
        private ZonedTrafficMap _vismap;
        private List<Hospital> _hospitals;
        private List<AmbulanceStation> amb_stations;
////        private PulseMapData _trafficMap;

//        private string _scenario;
//        private CoordType _coordSystem;

        #endregion

        private TrafficBinarySerializer _pba;
//        private ISnapshot _currentSnapShot;

        private PointsGisLayer agentsLayer;
        private PointsGisLayer emcLayer;
        private PointsGisLayer AmbStatoins;
        private PointsGisLayer Hospitals;

        private PointsGisLayer startCalls;
        private PointsGisLayer serviceCalls;
        private PointsGisLayer finishCalls;
//        private ModelLayer subwayAgents;

        //TODO remove this shit and all references (demo kostyl)
        //private PulseServer _tempSrv;

//        private TilesGisLayer tileMap;
//        private PolyGisLayer polyMap;
//        private PolyGisLayer krestMap;
//        private PolyGisLayer itmoBilds;
        private LinesGisLayer graphLayer;
        private LinesGisLayer emcPathsLayer;
        private LinesGisLayer almazTrueLayer;
        private LinesGisLayer selectedLines;

        private int itercount =0;

        private Frame callsPlot;
        private Frame arrivalPlot;
        private Frame finishTimePlot;

        private bool graphInitialize = false;

        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeControl()
        {
            //_testLayer = new SpriteLayer(Game.RenderSystem, 1024);
            Textures = new ConcurrentDictionary<string, DiscTexture>();
            Textures["circle"] = Game.Content.Load<DiscTexture>("a");
            Textures["black"] = Game.Content.Load<DiscTexture>("b");
            Textures["emc"] = Game.Content.Load<DiscTexture>("car7_7");
            Textures["hospital"] = Game.Content.Load<DiscTexture>("hospital");
            Textures["station"] = Game.Content.Load<DiscTexture>("station");
            Textures["start"] = Game.Content.Load<DiscTexture>("start");
            Textures["service"] = Game.Content.Load<DiscTexture>("service");
            Textures["finish"] = Game.Content.Load<DiscTexture>("finish");
            _pba = new TrafficBinarySerializer();

            //_masterLayer.SpriteLayers.Add(_testLayer);



            //_tempSrv = new PulseServer();
            //_tempSrv.Initialize();
        }

        protected override void LoadControl(ControlInfo serverInfo)
        {
            base.LoadLevel(serverInfo);

            var staticFactory = new StaticDataFactory();
            _map = staticFactory.GetScenarioMap(serverInfo.Scenario);
            _vismap = staticFactory.GetScenarioVisMap(serverInfo.Scenario);

            amb_stations = staticFactory.GetStations(serverInfo.Scenario);
            _hospitals = staticFactory.GetHospitals(serverInfo.Scenario);

            
            


            graphLayer = new LinesGisLayer(Game, 1200000);//grmap.Edges.Count+1);
            graphLayer.Flags = (int) (LinesGisLayer.LineFlags.DRAW_LINES);
            GisLayers.Add(graphLayer);
            var grmap = (_vismap);
            var gredges = grmap.Edges.Values.ToList();
                 
            for (int i = 0; i < gredges.Count; i++)
            {
                graphLayer.PointsCpu[2 * i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(gredges[i]
                    .StartPoint.Position.Longitude),
                    Lat = DMathUtil.DegreesToRadians(gredges[i]
                    .StartPoint.Position.Latitude),
                    Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Tex0 = new Vector4(0.005f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                };

                graphLayer.PointsCpu[2 * i + 1] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(gredges[i]
                    .EndPoint.Position.Longitude),
                    Lat = DMathUtil.DegreesToRadians(gredges[i]
                    .EndPoint.Position.Latitude),
                    Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Tex0 = new Vector4(0.005f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                };                
            }
            graphLayer.UpdatePointsBuffer();
            graphLayer.IsVisible = true;

            if (serverInfo.Scenario == "almazovskiy")
            {
                almazTrueLayer = new LinesGisLayer(Game, 1200000);//grmap.Edges.Count+1);
                almazTrueLayer.Flags = (int)(LinesGisLayer.LineFlags.DRAW_LINES);
                GisLayers.Add(almazTrueLayer);

                grmap = _map;
                gredges = grmap.Edges.Values.ToList();

                for(int i = 0; i < gredges.Count; i++)
                {
                    almazTrueLayer.PointsCpu[2 * i] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(gredges[i]
                    .StartPoint.Position.Longitude),
                        Lat = DMathUtil.DegreesToRadians(gredges[i]
                    .StartPoint.Position.Latitude),
                        Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Tex0 = new Vector4(0.005f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                    };

                    almazTrueLayer.PointsCpu[2 * i + 1] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(gredges[i]
                        .EndPoint.Position.Longitude),
                        Lat = DMathUtil.DegreesToRadians(gredges[i]
                        .EndPoint.Position.Latitude),
                        Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Tex0 = new Vector4(0.005f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                    };
                }
                almazTrueLayer.UpdatePointsBuffer();
                almazTrueLayer.IsVisible = true;
            }

                emcPathsLayer = new LinesGisLayer(Game, 20000);
            emcPathsLayer.Flags = (int) (LinesGisLayer.LineFlags.DRAW_LINES );
            GisLayers.Add(emcPathsLayer);

            agentsLayer = new PointsGisLayer(Game, 30000, false)
            {
                ImageSizeInAtlas = new Vector2(36, 36),
                TextureAtlas = Game.Content.Load<Texture2D>("circles.tga")
            };

            GisLayers.Add(agentsLayer);

            emcLayer = new PointsGisLayer(Game, 50, false)
            {
                ImageSizeInAtlas = new Vector2(794, 794),
                TextureAtlas = Game.Content.Load<Texture2D>("car7_7.png")
            };
            GisLayers.Add(emcLayer);

            AmbStatoins = new PointsGisLayer(Game, 50, false)
            {
                ImageSizeInAtlas = new Vector2(131, 131),
                TextureAtlas = Game.Content.Load<Texture2D>("station.png")
            };
            GisLayers.Add(AmbStatoins);

            Hospitals = new PointsGisLayer(Game, 50, false)
            {
                ImageSizeInAtlas = new Vector2(131, 131),
                TextureAtlas = Game.Content.Load<Texture2D>("hospital.png")
            };
            GisLayers.Add(Hospitals);

            startCalls = new PointsGisLayer(Game, 150, false)
            {
                ImageSizeInAtlas = new Vector2(48, 48),
                TextureAtlas = Game.Content.Load<Texture2D>("start.png")
            };
            GisLayers.Add(startCalls);

            serviceCalls = new PointsGisLayer(Game, 150, false)
            {
                ImageSizeInAtlas = new Vector2(48, 48),
                TextureAtlas = Game.Content.Load<Texture2D>("service.png")
            };
            GisLayers.Add(serviceCalls);

            finishCalls = new PointsGisLayer(Game, 150, false)
            {
                ImageSizeInAtlas = new Vector2(48, 48),
                TextureAtlas = Game.Content.Load<Texture2D>("finish.png")
            };
            GisLayers.Add(finishCalls);

            //            graphLayer = new LinesGisLayer(Game, 4);
            //            graphLayer.Flags = (int)(LinesGisLayer.LineFlags.DRAW_LINES | LinesGisLayer.LineFlags.ADD_CAPS);


            //            _masterLayer.GisLayers.Add(graphLayer);

            selectedLines = new LinesGisLayer(Game, 100);//grmap.Edges.Count+1);
            selectedLines.Flags = (int)(LinesGisLayer.LineFlags.DRAW_LINES | LinesGisLayer.LineFlags.ADD_CAPS);
            GisLayers.Add(selectedLines);

            List<Vector2> listGraph = new List<Vector2>();
            List<Vector2> listGraph1 = new List<Vector2>();
            List<Vector2>listGraph2 = new List<Vector2>();
            /*for (float i = -10; i <= 15; i += 0.1f)
            {
                listGraph.Add(new Vector2(i, (float)Math.Sin(i)));
                listGraph1.Add(new Vector2(i, -(float)Math.Sin(i / 2)));
            }*/
            FrameProcessor ui = (Game.GameInterface as CustomGameInterface).ui;
            callsPlot = FrameHelper.createPlot(ui, 0, 0, 300, 300, new List<List<Vector2>>()
            {
                listGraph,listGraph1,listGraph2
            },"calls count" ,"calls count", "time (seconds)", ColorConstant.defaultConfigPlot);
            
            //var frameWithPlot = FrameHelper.addHeaderToFrame(ui, callsPlot);
            //ui.RootFrame.Add(frameWithPlot);
            

            ui.RootFrame.Add(callsPlot);

            List<Vector2> arrivalData = new List<Vector2>();

            arrivalPlot = FrameHelper.createPlot(ui, 0, 350, 300, 300, new List<List<Vector2>>()
            {
                arrivalData,
            }, "arrival time", "arrival time", "time (seconds)", ColorConstant.defaultConfigPlot);
            ui.RootFrame.Add(arrivalPlot);

            List<Vector2> finishPlotData = new List<Vector2>();
            
            finishTimePlot = FrameHelper.createPlot(ui, 0, 700, 300, 300, new List<List<Vector2>>()
            {
                finishPlotData,
            }, "finish time"  , "finish time", "time (seconds)", ColorConstant.defaultConfigPlot);
            ui.RootFrame.Add(finishTimePlot);


            var bar = FrameHelper.createBarChart(ui, 100, 100, 500, 500, new List<List<BarChart.PairCoor>>()
            {
                new List<BarChart.PairCoor>()
                {
                   new BarChart.PairCoor("10", 100),
                  new BarChart.PairCoor("200", 200),
                   new BarChart.PairCoor("300", 300),
                   new BarChart.PairCoor("400", 400),
                   new BarChart.PairCoor("5", 250),
                   new BarChart.PairCoor("400", 50),
                   new BarChart.PairCoor("400", 100),

                }
            },"1","1","1");

           // ui.RootFrame.Add(bar);
           /*
            List<Vector2> listGraph11 = new List<Vector2>();
            List<Vector2> listGraph111 = new List<Vector2>();
            for (float i = -10; i <= 15; i += 0.1f)
            {
                listGraph11.Add(new Vector2(i, (float)Math.Sin(i)));
                listGraph111.Add(new Vector2(i, -(float)Math.Sin(i / 2)));
            }
            Frame plot = FrameHelper.createPlot(ui, 100, 100, 500, 500, new List<List<Vector2>>()
{
 listGraph,
 listGraph1,
},"2","2","2");
            ui.RootFrame.Add(plot); */

        }
/*
        private void Configure(PulseScenarioConfig scenarioConfig)
        {
            _coordSystem = scenarioConfig.PreferredCoordinates.Value == "geo" ? CoordType.Geo : CoordType.Map;
        }
*/
//        public void UnloadLevel()
//        {
//        }

//        public void FeedSnapshot(byte[] snapshot)
//        {
//            Cur
            //_currentSnapShot = _pba.DeserializeSnapShot(_tempSrv.Update());
//        }

        private int previousPathsCount = 0;

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            //_testLayer.Clear();

            var s = CurrentSnapshot as TrafficSnapshot;
            

            if (s != null)
            {
                for (int i = 0; i < s.EmcPaths.EmcPaths.Count; i++)
                {
                    uint id = s.EmcPaths.EmcPaths[i];
                    emcPathsLayer.PointsCpu[2*i] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(_map.Edges[id]
                                .StartPoint.Position.Longitude),
                        Lat = DMathUtil.DegreesToRadians(_map.Edges[id]
                                .StartPoint.Position.Latitude),
                        Color = new Color4(1f, 0.5f, 0f, 1.0f),
                        Tex0 = new Vector4(0.01f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                    };

                    emcPathsLayer.PointsCpu[2*i+1] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(_map.Edges[id]
                                .EndPoint.Position.Longitude),
                        Lat = DMathUtil.DegreesToRadians(_map.Edges[id]
                                .EndPoint.Position.Latitude),
                        Color = new Color4(1f, 0.5f, 0f, 1.0f),
                        Tex0 = new Vector4(0.01f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                    };
                    
                }

                if (s.EmcPaths.EmcPaths.Count < previousPathsCount)
                {
                    for (int i = 2*s.EmcPaths.EmcPaths.Count; i < emcPathsLayer.PointsCpu.Length; i++)
                    {
                        emcPathsLayer.PointsCpu[i] = new Gis.GeoPoint();
                    }
                }

                previousPathsCount = s.EmcPaths.EmcPaths.Count;


                emcPathsLayer.UpdatePointsBuffer();
                emcPathsLayer.IsVisible = true;
                    
                if (ControlConfig.Scenario.ToLower() == "simpletraffic")
                {
                    for (int i = 0; i < s.Agents.Count; i++)
                    {
                        agentsLayer.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(s.Agents[i].Y),
                            Lat = DMathUtil.DegreesToRadians(s.Agents[i].X),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.002f, 3.14f)
                        };

                    }
                    agentsLayer.UpdatePointsBuffer();
                    agentsLayer.PointsCountToDraw = s.Agents.Count;
                    agentsLayer.IsVisible = true;
                }

                else if (ControlConfig.Scenario.ToLower() == "vasilevskiy" || ControlConfig.Scenario.ToLower() == "almazovskiy")
                {
                    for(int i=0; i < s.Agents.Count; i++)
                    {
                        emcLayer.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(s.Agents[i].Y),
                            Lat = DMathUtil.DegreesToRadians(s.Agents[i].X),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    emcLayer.UpdatePointsBuffer();
                    emcLayer.PointsCountToDraw = s.Agents.Count;
                    emcLayer.IsVisible = true;

                    for(int i=0; i < amb_stations.Count; i ++)
                    {
                        AmbulanceStation amb_station = amb_stations[i];
                        AmbStatoins.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(amb_station.Niarest.Position.Longitude),
                            Lat = DMathUtil.DegreesToRadians(amb_station.Niarest.Position.Latitude),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    AmbStatoins.UpdatePointsBuffer();
                    AmbStatoins.PointsCountToDraw = amb_stations.Count;
                    AmbStatoins.IsVisible = true;

                    for(int i = 0; i< _hospitals.Count; i++)
                    {
                        Hospital curHosp = _hospitals[i];
                        Hospitals.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(curHosp.Nearest.Position.Longitude),
                            Lat = DMathUtil.DegreesToRadians(curHosp.Nearest.Position.Latitude),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    Hospitals.UpdatePointsBuffer();
                    Hospitals.PointsCountToDraw = _hospitals.Count;

                    Hospitals.IsVisible = true;

#region callsLayers
                    var stcalls =
                        s.Calls.Where(c => StaticDataFactory.ConvertIntToCallState(c.callState) == CallState.Expectations).ToList();
                    for (int i = 0; i < stcalls.Count; i++)
                    {
                        SimpleTraffic.Node patientNode= _map.Nodes[stcalls[i].nodeId];
                        startCalls.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(patientNode.Position.Longitude),
                            Lat = DMathUtil.DegreesToRadians(patientNode.Position.Latitude),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    startCalls.UpdatePointsBuffer();
                    startCalls.PointsCountToDraw = stcalls.Count;

                    startCalls.IsVisible = true;

                    var srvcalls =
                        s.Calls.Where(c => StaticDataFactory.ConvertIntToCallState(c.callState) == CallState.Service).ToList();
                    for (int i = 0; i < srvcalls.Count; i++)
                    {
                        SimpleTraffic.Node patientNode = _map.Nodes[srvcalls[i].nodeId];
                        serviceCalls.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(patientNode.Position.Longitude),
                            Lat = DMathUtil.DegreesToRadians(patientNode.Position.Latitude),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    serviceCalls.UpdatePointsBuffer();
                    serviceCalls.PointsCountToDraw = srvcalls.Count;

                    serviceCalls.IsVisible = true;

                    var fincalls =
                        s.Calls.Where(c => StaticDataFactory.ConvertIntToCallState(c.callState) == CallState.Finish).ToList();
                    for (int i = 0; i < fincalls.Count; i++)
                    {
                        SimpleTraffic.Node patientNode = _map.Nodes[fincalls[i].nodeId];
                        finishCalls.PointsCpu[i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(patientNode.Position.Longitude),
                            Lat = DMathUtil.DegreesToRadians(patientNode.Position.Latitude),
                            Color = Color.White,
                            Tex0 = new Vector4(0, 0, 0.2f, 3.14f)
                        };
                    }
                    finishCalls.UpdatePointsBuffer();
                    finishCalls.PointsCountToDraw = fincalls.Count;

                    finishCalls.IsVisible = true;
#endregion

                    for (int i = 0; i < s.SelectLines.Count; i++)
                    {
                        selectedLines.PointsCpu[2 * i] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(s.SelectLines[i].emcLon),
                            Lat = DMathUtil.DegreesToRadians(s.SelectLines[i].emcLat),
                            Color = new Color4(0f, 1f, 1f, 1.0f),
                            Tex0 = new Vector4(0.01f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                        };

                        selectedLines.PointsCpu[2 * i + 1] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(s.SelectLines[i].hospLon),
                            Lat = DMathUtil.DegreesToRadians(s.SelectLines[i].hospLat),
                            Color = new Color4(0f, 1f, 1f, 1.0f),
                            Tex0 = new Vector4(0.01f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                        };
                    }
                    selectedLines.UpdatePointsBuffer();

                    selectedLines.IsVisible = s.SelectLines.Count != 0;
                }

                float time = s.emc_Stats.IterationCount * (float) 0.3;
                
                (callsPlot as GraphicBase).addPointToPlot(new Vector2(time, s.emc_Stats.AllCallsNumber),0);
                (callsPlot as GraphicBase).addPointToPlot(new Vector2(time, s.emc_Stats.CurrCallsNumb),1);
                (callsPlot as GraphicBase).addPointToPlot(new Vector2(time, s.emc_Stats.FinishCallsNumb),2);
                //(Game.GameInterface as CustomGameInterface).ui.Update(gameTime);

                (arrivalPlot as GraphicBase).addPointToPlot(new Vector2(s.emc_Stats.IterationCount, (float) s.emc_Stats.AvgArrivalTime));
                (finishTimePlot as GraphicBase).addPointToPlot(new Vector2(s.emc_Stats.IterationCount, (float) s.emc_Stats.AvgFinishTime));
                itercount += 1; 
            }
            agentsLayer.IsVisible = true;
                        
            return null;
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
        }

        public ISnapshot CurrentSnapshot { get; set; }

        //TODO userinfo & serverinfo protocol
        public override string UserInfo()
        {
            return "Todo: userinfo & serverinfo protocol";
        }
    }
}

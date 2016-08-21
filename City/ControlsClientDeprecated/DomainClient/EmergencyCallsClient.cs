using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.Linq;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace City.ControlsClient.DomainClient
{
    public class EmergencyCallsClient : AbstractPulseClient
    {
        private PointsGisLayer globeCalls;
        private List<EmcCall> allCalls;
        private List<MunPolygon> mpols;
        private List<PolyGisLayer> polyLayers;
        private List<PolyGisLayer> _morning = new List<PolyGisLayer>();

        private List<DataPoint> AllCalls;

        private PointsGisLayer dinamicCalls;

        private double Speed = 0.02f; //0.1f;
        private double tt = 0;

        private List<Color> PaletteColors = new List<Color>()
        {
            new Color(20, 176, 129),
            new Color(141, 212, 134),
            new Color(253, 228, 112),
            new Color(253, 184, 93),
            new Color(247, 70, 98)
        };

        private List<Vector3> PaletteCoords = new List<Vector3>()
        {
            new Vector3(20, 176, 129),
            new Vector3(141, 212, 134),
            new Vector3(253, 228, 112),
            new Vector3(253, 184, 93),
            new Vector3(247, 70, 98)
        };

        private Dictionary<string, List<PolyGisLayer>> dinamicPolyLayers;

        private LinesGisLayer dinamicLinesLayer;

        private Dictionary<string, MunPolygon> dinamicPolygons;

        private List<List<PolyGisLayer>> _polygonIntervals = new List<List<PolyGisLayer>>();
        private List<LinesGisLayer> _boundsInterval = new List<LinesGisLayer>();

        private Game _engine;
        /*for write missing*/

        private int time = 0;

        //        public override void Initialize(Game gameEngine)
        //        {
        //            base.Initialize(gameEngine);
        //            _engine = gameEngine;
        //        }

        public override Frame AddControlsToUI()
        {
            if (!(Game.GameInterface is CustomGameInterface))
                return null;
            var ui = ((CustomGameInterface) Game.GameInterface).ui;
            //            var scenarioFrame = FrameHelper.createControlFrame(ui);
            var controlElements = Generator.getControlElement(new SentimentControlUI(globeCalls), ui);
            //            scenarioFrame.transition(true);
            return controlElements;
        }

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo serverInfo)
        {
            base.LoadLevel(serverInfo);
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

            //1. read polygons
            List<MunPolygon> mpols =
                File.ReadAllLines(dir + @"SaintPetersburg\" + "0-6_mundata.txt", Encoding.GetEncoding(1251))
                    .Skip(2)
                    .Select(a =>
                    {
                        return ReadPolygon(a);
                    }).ToList();

            //2. get polygons density
            Dictionary<string, MunPolygon> PolyDict = new Dictionary<string, MunPolygon>();
            foreach (var munPolygon in mpols)
            {
                PolyDict.Add(munPolygon.Name, munPolygon);
                //Console.WriteLine(munPolygon.Name);
            } //mpols.ToDictionary(a => a.Name, a => a);
            GetPolygonsDensity(PolyDict);

            //3. Yandex data
            GetTimesToHospitals(PolyDict);

            //4. Calls count in polygons
            AllCalls = GetCallsCountInPolygons(PolyDict);



            //6 counting life factors
            GetLifeFactorsInDataPoints(PolyDict);

            dinamicPolygons = PolyDict;

            dinamicPolyLayers = GenerateMainPolyLayers();

            //5 dinamic calls layer
            dinamicCalls = new PointsGisLayer(Game, AllCalls.Count)
            {
                ImageSizeInAtlas = new Vector2(512, 512),
                TextureAtlas = Game.Content.Load<Texture2D>("big_circle.tga")
            };

            GisLayers.Add(dinamicCalls);

            var calls = File.ReadAllLines(dir + @"SaintPetersburg\emergency_calls.csv").Skip(1).Select(a =>
            {
                var rawPost = a.Split(',');
                return new EmcCall
                {
                    Lat = Double.Parse(rawPost[0], CultureInfo.InvariantCulture),
                    Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
                };
            }).ToList();

            allCalls = calls;

            //masterView.GisLayers.Add(new PolyGisLayer(GameEngine, points, indeces));
            //var first_time = GeneratePolygonsWithFile("0-6_mundata.txt");
            //var sec_time = GeneratePolygonsWithFile("6-12_mundata.txt");
            //var third_time = GeneratePolygonsWithFile("12-18_mundata.txt");
            //var fourth_time = GeneratePolygonsWithFile("18-24_mundata.txt");

            /* 
             globeCalls = new PointsGisLayer(Game, allCalls.Count)
             {
                 ImageSizeInAtlas = new Vector2(512, 512),
                 TextureAtlas = Game.Content.Load<Texture2D>("big_circle.tga")
             };

             for (int i = 0; i < allCalls.Count; i++)
             {
                 var call = allCalls[i];
                 Color4 color = new Color4(0.8f, 0.8f, 0.8f, 1); // new Color4(0f, 0.74f, 1.0f, 1.0f);

                 //color = Color.Red;
                 globeCalls.PointsCpu[i] = new Gis.GeoPoint
                 {
                     Lon = DMathUtil.DegreesToRadians(call.Lon),
                     Lat = DMathUtil.DegreesToRadians(call.Lat),
                     Color = color,
                     Tex0 = new Vector4(0, 0, 0.2f , 0.0f)
                 };
             }

             globeCalls.UpdatePointsBuffer();

             MasterLayer.GisLayers.Add(globeCalls);

             drawContolUI();

             */
        }

        public override void UnloadLevel()
        {
            base.UnloadLevel();
        }

        private int alpha = 0;
        private DateTime curTime = new DateTime(2015, 1, 1, 0, 0, 0);

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            tt += gameTime.ElapsedSec;

            if (tt > Speed)
            {
                tt -= Speed;

                int curline_number = 0;

                //update polygons every 10 minutes
                if (curTime.Minute%6 == 0)
                {
                    //update lines layer
                    foreach (var munPolygon in dinamicPolygons.Values)
                    {
                        for (int j = 0; j < munPolygon.Points.Count; j++)
                        {
                            if (munPolygon.Points[j].Count == 0) continue;

                            for (int i = 0; i < munPolygon.Points[j].Count; i++)
                            {
                                int currColor;
                                double lfact = munPolygon.EmcWorkFactors[time%24];

                                if (lfact*300 >= 0.8) currColor = 4;

                                else if (lfact*300 >= 0.6) currColor = 3;

                                else if (lfact*300 >= 0.4) currColor = 2;

                                else if (lfact*300 >= 0.2) currColor = 1;

                                else currColor = 0;

                                int lastColor;
                                int hours = time == 0 ? 23 : (time - 1)%24;
                                double lastfact = munPolygon.EmcWorkFactors[hours];

                                if (lastfact*300 >= 0.8) lastColor = 4;

                                else if (lastfact*300 >= 0.6) lastColor = 3;

                                else if (lastfact*300 >= 0.4) lastColor = 2;

                                else if (lastfact*300 >= 0.2) lastColor = 1;

                                else lastColor = 0;

                                Vector3 interval = (PaletteCoords[currColor] - PaletteCoords[lastColor])/10;
                                Vector3 final_vect = PaletteCoords[lastColor] + interval*alpha;

                                Color final_Color = new Color((int) final_vect.X, (int) final_vect.Y, (int) final_vect.Z);

                                dinamicLinesLayer.PointsCpu[2*curline_number].Color = final_Color;

                                dinamicLinesLayer.PointsCpu[2*curline_number + 1].Color = final_Color;

                                curline_number += 1;
                            }
                        }
                    }



                    dinamicLinesLayer.UpdatePointsBuffer();
                    dinamicLinesLayer.IsVisible = true;

                    foreach (var polyList in dinamicPolyLayers)
                    {
                        foreach (var poly in polyList.Value)
                        {
                            int hours = time == 0 ? 23 : (time - 1)%24;
                            double resultfact =
                                ((dinamicPolygons[polyList.Key].EmcWorkFactors[(time)%24] -
                                  dinamicPolygons[polyList.Key].EmcWorkFactors[hours])/10)*alpha;
                            poly.PaletteValue =
                                (float) (dinamicPolygons[polyList.Key].EmcWorkFactors[hours]*300 + resultfact*300);
                            poly.IsVisible = true;

                            poly.UpdatePointsBuffer();
                        }

                    }
                    alpha += 1;

                    //Log.Message("alpha "+ alpha + " time " + time % 24);
                    if (alpha > 10)
                    {

                        time += 1;
                        Log.Message("time" + time%24);
                        alpha = 0;
                    }

                }

                DateTime lastBound = new DateTime(curTime.Year, curTime.Month, curTime.Day, curTime.Hour, curTime.Minute,
                    0);
                lastBound = lastBound.AddMinutes(60);

                List<DataPoint> curpoints =
                    AllCalls.Where(
                        callpoint =>
                            ((callpoint.CallsTime.Minute >= curTime.Minute && callpoint.CallsTime.Hour == curTime.Hour)
                             || callpoint.CallsTime.Hour > curTime.Hour) &&
                            ((callpoint.CallsTime.Minute <= lastBound.Minute &&
                              callpoint.CallsTime.Hour == lastBound.Hour)
                             || callpoint.CallsTime.Hour < lastBound.Hour)).ToList();

                Color4 color = new Color4(0.8f, 0.8f, 0.8f, 1); // new Color4(0f, 0.74f, 1.0f, 1.0f);
                for (int i = 0; i < curpoints.Count; i++)
                {
                    dinamicCalls.PointsCpu[i] = new Gis.GeoPoint()
                    {
                        Lon = DMathUtil.DegreesToRadians(curpoints[i].Lon),
                        Lat = DMathUtil.DegreesToRadians(curpoints[i].Lat),
                        Color = color,
                        Tex0 = new Vector4(0, 0, 0.2f, 0.0f)
                    };
                }

                dinamicCalls.UpdatePointsBuffer();

                curTime = curTime.AddMinutes(1);
            }



            //dinamicLinesLayer.UpdatePointsBuffer();
            return null;
        }

        public override string UserInfo()
        {
            return $"ACS calls count: {allCalls.Count}";
        }

        public MunPolygon ReadPolygon(string line)
        {
            MunPolygon currentMunitipal = new MunPolygon();
            var rawData = line.Split(',');
            if (rawData[0].Contains("MultiPolygon"))
            {
                rawData[0] =
                    rawData[0].Replace("MultiPolygon ", "")
                        .Replace("(((", "((")
                        .Replace(")))", "))")
                        .Replace("((", "")
                        .Replace("))", ",");
                List<string> polygonsData = rawData[0].Split(',').ToList();
                foreach (var polygon in polygonsData)
                {
                    currentMunitipal.Points.Add(ReadSimplePolygon(polygon));
                }
            }
            else
            {
                rawData[0] = rawData[0].Replace("Polygon ", "").Replace("((", "").Replace("))", ",");
                currentMunitipal.Points.Add(ReadSimplePolygon(rawData[0]));
            }

            //currentMunitipal.Lifefactor = double.Parse(rawData[8], CultureInfo.InvariantCulture);
            //currentMunitipal.Intensive = double.Parse(rawData[3], CultureInfo.InvariantCulture);
            //currentMunitipal.NormCoeff = int.Parse(rawData[11]);
            //currentMunitipal.NotNormCoeff = int.Parse(rawData[13]);
            currentMunitipal.Name = rawData[2];
            return currentMunitipal;
        }

        public List<GeoCoordinate> ReadSimplePolygon(string polygon)
        {
            List<GeoCoordinate> polyPoints = new List<GeoCoordinate>();
            List<string> pointsData = polygon.Split(';').ToList();
            foreach (var point in pointsData)
            {
                if (point == "") continue;
                string newpoint = point[0] == ' ' ? point.Remove(0, 1) : point;
                var s = newpoint.Split(' ');
                GeoCoordinate newPolyPoint =
                    new GeoCoordinate(
                        double.Parse(newpoint.Split(' ')[1].Replace(",", " "), CultureInfo.InvariantCulture),
                        double.Parse(newpoint.Split(' ')[0], CultureInfo.InvariantCulture));
                polyPoints.Add(newPolyPoint);
            }

            return polyPoints;
        }

        public List<PolyGisLayer> GeneratePolygonsWithFile(string filename)
        {
            List<PolyGisLayer> curtimePolygons = new List<PolyGisLayer>();

            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

            List<MunPolygon> mpols = File.ReadAllLines(dir + @"SaintPetersburg\" + filename).Skip(2).Select(a =>
            {
                return ReadPolygon(a);
            }).ToList();


            foreach (var munPolygon in mpols)
            {
                for (int j = 0; j < munPolygon.Points.Count; j++)
                {
                    if (munPolygon.Points[j].Count == 0) continue;

                    var polyCoords = new DVector2[10000];
                    for (int i = 0; i < munPolygon.Points[j].Count; i++)
                    {
                        GeoCoordinate cur_point = munPolygon.Points[j][i];
                        polyCoords[i] = new DVector2(DMathUtil.DegreesToRadians(cur_point.Longitude),
                            DMathUtil.DegreesToRadians(cur_point.Latitude));
                    }

                    var cont = PolyGisLayer.CreateFromContour(Game, polyCoords, Color.Red);
                    cont.Texture = Game.Content.Load<Texture2D>("palette_v3");
                    cont.PaletteTransparency = 0.35f;
                    cont.PaletteValue = (float) (munPolygon.Lifefactor*1000);

                    //0.4f + (0.1f*munPolygon.NormCoeff);
                    cont.IsVisible = false;
                    GisLayers.Add(cont);
                    curtimePolygons.Add(cont);
                    _morning.Add(cont);
                }
            }

            _polygonIntervals.Add(curtimePolygons);


            int pointscount = GetPoligonsPointsCount(mpols);

            var bounds = new LinesGisLayer(Game, 2*pointscount + 1, true);
            bounds.Flags = (int) (LinesGisLayer.LineFlags.DRAW_LINES);
            int curline_number = 0;
            //palette rgb
            //247,70,98
            //253,184,93
            //253,228,112
            //141,212,134
            //20,176,129


            foreach (var munPolygon in mpols)
            {
                for (int j = 0; j < munPolygon.Points.Count; j++)
                {
                    if (munPolygon.Points[j].Count == 0) continue;

                    for (int i = 0; i < munPolygon.Points[j].Count; i++)
                    {
                        GeoCoordinate cur_point = munPolygon.Points[j][i];
                        GeoCoordinate nextpoint = i == munPolygon.Points[j].Count - 1
                            ? munPolygon.Points[j][0]
                            : munPolygon.Points[j][i + 1];

                        Color clColor = Color.Aqua;
                        if (munPolygon.Lifefactor*1000 >= 0.8) clColor = new Color(247, 70, 98);

                        else if (munPolygon.Lifefactor*1000 >= 0.6) clColor = new Color(253, 184, 93);

                        else if (munPolygon.Lifefactor*1000 >= 0.4) clColor = new Color(253, 228, 112);

                        else if (munPolygon.Lifefactor*1000 >= 0.2) clColor = new Color(141, 212, 134);

                        else clColor = new Color(20, 176, 129);


                        bounds.PointsCpu[2*curline_number] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(cur_point.Longitude),
                            Lat = DMathUtil.DegreesToRadians(cur_point.Latitude),
                            Color = clColor, //new Color4(0.8f,0.8f,0.8f,1),//new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                            Tex0 = new Vector4(0.02f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                        };

                        bounds.PointsCpu[2*curline_number + 1] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(nextpoint.Longitude),
                            Lat = DMathUtil.DegreesToRadians(nextpoint.Latitude),
                            Color = clColor, //new Color4(0.8f, 0.8f, 0.8f, 1),//new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                            Tex0 = new Vector4(0.02f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                        };

                        curline_number += 1;
                    }

                }
            }
            bounds.UpdatePointsBuffer();
            bounds.IsVisible = false;
            GisLayers.Add(bounds);

            _boundsInterval.Add(bounds);


            return curtimePolygons;
        }

        public int GetPoligonsPointsCount(List<MunPolygon> pols)
        {
            int count = 0;
            foreach (var munPolygon in pols)
            {
                foreach (var poly in munPolygon.Points)
                {
                    count += poly.Count;
                }
            }
            return count;
        }

        public void GetPolygonsDensity(Dictionary<string, MunPolygon> munPols)
        {
            for (int i = 0; i < 24; i++)
            {
                var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

                var density = File.ReadAllLines(dir + @"SaintPetersburg\emc_density\" + i + "finalData.txt").Select(a =>
                {

                    var data = a.Split(';');

                    munPols[data[0]].PeopleDensity.Add(double.Parse(data[1].Replace(",", "."),
                        CultureInfo.InvariantCulture));
                    return true;
                }).ToList();
            }
        }

        public void GetTimesToHospitals(Dictionary<string, MunPolygon> munPols)
        {
            for (int i = 0; i < 24; i++)
            {
                var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

                //get time points
                var time_points =
                    File.ReadAllLines(dir + @"SaintPetersburg\emc_time_to_hospital\" + i + ".txt").Skip(1).Select(a =>
                    {
                        int index = a.LastIndexOf('"');
                        var data = a.Remove(0, index + 1);
                        var gooddata = data.Split(',');
                        //var data = a.Split('"');
                        //var gooddata = a.Contains("Аэропорт Пулково-1") || a.Contains("площадь Пулково-1") ? data[2].Split(',') : data[4].Split(',');
                        var point = new DataPoint(double.Parse(gooddata[1], CultureInfo.InvariantCulture),
                            double.Parse(gooddata[2], CultureInfo.InvariantCulture),
                            double.Parse(gooddata[7], CultureInfo.InvariantCulture));
                        return point;

                    }).ToList();

                //get all points in polygons
                var timesDict = GetPointsInPolygons(munPols, time_points);

                // get time info in polygons
                foreach (var poly in munPols.Values)
                {
                    double averageTime = 0;
                    double pointscount = 0;
                    foreach (var dataPoint in timesDict[poly.Name])
                    {
                        averageTime += dataPoint.HospTime;
                        pointscount += 1;
                    }

                    averageTime = pointscount != 0 ? averageTime/pointscount : pointscount;

                    poly.AverageHospitalTimes.Add(averageTime);
                }



            }
        }

        public class DataPoint
        {
            public double Lat { get; set; }
            public double Lon { get; set; }

            public double HospTime { get; set; }
            public int callsHours { get; set; }

            public DateTime CallsTime { get; set; }

            public DataPoint(double lat, double lon, double hospTime)
            {
                Lat = lat;
                Lon = lon;
                HospTime = hospTime;
            }

            public DataPoint()
            {

            }

        }

        public Dictionary<string, List<DataPoint>> GetPointsInPolygons(Dictionary<string, MunPolygon> munPols,
            List<DataPoint> allDataPoints)
        {
            Dictionary<string, List<DataPoint>> pointsInPolygons = new Dictionary<string, List<DataPoint>>();
            foreach (var key in munPols.Keys)
            {
                pointsInPolygons.Add(key, new List<DataPoint>());
            }

            foreach (var poly in munPols.Values)
            {
                foreach (var point in allDataPoints)
                {
                    if (IsPtInPolygon(poly, point))
                    {
                        pointsInPolygons[poly.Name].Add(point);
                    }
                }
            }

            return pointsInPolygons;
        }

        //подсчет количества значимых ребер полигона, для которых точка справа расположена
        public bool IsPtInPolygon(MunPolygon mpol, DataPoint my_point)
        {
            /*
            bool pt_in_polygon(const T &test,const std::vector &polygon)
            {
                if (polygon.size() < 3) return false;

                std::vector::const_iterator end = polygon.end();

                T last_pt = polygon.back();

                last_pt.x -= test.x;
                last_pt.y -= test.y;

                double sum = 0.0;

                for (
                std::vector::const_iterator iter = polygon.begin();
                iter != end;
                ++iter
                )
                {
                    T cur_pt = *iter;
                    cur_pt.x -= test.x;
                    cur_pt.y -= test.y;

                    double del = last_pt.x * cur_pt.y - cur_pt.x * last_pt.y;
                    double xy = cur_pt.x * last_pt.x + cur_pt.y * last_pt.y;

                    sum +=
                    (
                    atan((last_pt.x * last_pt.x + last_pt.y * last_pt.y - xy) / del) +
                    atan((cur_pt.x * cur_pt.x + cur_pt.y * cur_pt.y - xy) / del)
                    );

                    last_pt = cur_pt;

                }

                return fabs(sum) > eps;
            }
           */

            /*
            function MIN(a, b){ return (a < b) ? a : b; } function MAX(a, b){ return (a > b) ? a : b; }

            function isInPoly(polygon, point)
            {
                var i = 1, N = polygon.length, isIn = false,
                    p1 = polygon[0], p2;

                for (; i <= N; i++)
                {
                    p2 = polygon[i % N];
                    if (point.lng > MIN(p1.lng, p2.lng))
                    {
                        if (point.lng <= MAX(p1.lng, p2.lng))
                        {
                            if (point.lat <= MAX(p1.lat, p2.lat))
                            {
                                if (p1.lng != p2.lng)
                                {
                                    xinters = (point.lng - p1.lng) * (p2.lat - p1.lat) / (p2.lng - p1.lng) + p1.lat;
                                    if (p1.lat == p2.lat || point.lat <= xinters)
                                        isIn = !isIn;
                                }
                            }
                        }
                    }
                    p1 = p2;
                }
                return isIn;
            }
            */

            //List<GeoCoordinate>[] cur_points = new List<GeoCoordinate>[] {};
            List<List<GeoCoordinate>> cur_points = new List<List<GeoCoordinate>>();

            foreach (var p in mpol.Points)
            {
                List<GeoCoordinate> cur_simple_points = new List<GeoCoordinate>();
                foreach (var curp in p)
                {
                    GeoCoordinate gk = new GeoCoordinate();
                    gk.Latitude = curp.Latitude;
                    gk.Longitude = curp.Longitude;
                    cur_simple_points.Add(gk);
                }
                cur_points.Add(cur_simple_points);
            }

            cur_points[0].RemoveAt(cur_points[0].Count - 1);

            foreach (var points in cur_points)
            {
                if (points.Count == 0) continue;
                bool InPoly = false;

                var last_pt = new GeoCoordinate(); //points[points.Count -1];
                last_pt.Latitude = points[points.Count - 1].Latitude;
                last_pt.Longitude = points[points.Count - 1].Longitude;

                //last_pt.Latitude -= my_point.Lat;
                //last_pt.Longitude -= my_point.Lon;

                double sum = 0;

                for (int i = 0; i < points.Count; i++)
                {
                    GeoCoordinate curPoint = points[i];
                    if (my_point.Lon > Math.Min(last_pt.Longitude, curPoint.Longitude))
                    {
                        if (my_point.Lon <= Math.Max(last_pt.Longitude, curPoint.Longitude))
                        {
                            if (my_point.Lat <= Math.Max(last_pt.Latitude, curPoint.Latitude))
                            {
                                if (curPoint.Longitude != last_pt.Longitude)
                                {
                                    var xinters = (my_point.Lon - last_pt.Longitude)*
                                                  (curPoint.Latitude - last_pt.Latitude)/
                                                  (curPoint.Longitude - last_pt.Longitude) + last_pt.Latitude;
                                    if (last_pt.Latitude == curPoint.Latitude || my_point.Lat <= xinters)
                                        InPoly = !InPoly;
                                }
                            }
                        }
                    }
                    last_pt = curPoint;
                }

                if (InPoly)
                    return InPoly;
                /*
                    GeoCoordinate curPoint = points[i];
                    curPoint.Latitude -= my_point.Lat;
                    curPoint.Longitude -= my_point.Lon;

                    double del = last_pt.Latitude * curPoint.Longitude - curPoint.Latitude * last_pt.Longitude;

                        double xy = curPoint.Latitude*last_pt.Latitude + curPoint.Longitude*last_pt.Longitude;
                        sum += Math.Atan((Math.Pow(last_pt.Latitude, 2) + Math.Pow(last_pt.Longitude, 2) - xy)/del) +
                               Math.Atan((Math.Pow(curPoint.Latitude, 2) + Math.Pow(curPoint.Longitude, 2) - xy)/del);


                    last_pt = curPoint;
                }
                if (Math.Abs(sum) > Double.Epsilon)
                {
                    return true;
                }
                */
            }

            return false;
        }

        //return calls info and counting the calls in polygons
        public List<DataPoint> GetCallsCountInPolygons(Dictionary<string, MunPolygon> munpols)
        {
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

            //read calls
            var calls = File.ReadAllLines(dir + @"SaintPetersburg\emergency_calls.csv").Skip(1).Select(a =>
            {
                var rawPost = a.Split(',');
                string time_data = rawPost[3];

                var data = time_data.Split(' ')[0];
                var time = time_data.Split(' ')[1];

                var datamas = data.Split('.');
                var timemas = time.Split(':');

                DateTime t = new DateTime();
                return new DataPoint()
                {
                    Lat = Double.Parse(rawPost[0], CultureInfo.InvariantCulture),
                    Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
                    CallsTime = new DateTime(int.Parse(datamas[2]), int.Parse(datamas[1]), int.Parse(datamas[0]),
                        int.Parse(timemas[0]), int.Parse(timemas[1]), int.Parse(timemas[2])),
                };
            }).ToList();

            for (int i = 0; i < 24; i++)
            {
                var PointsInPolygons = GetPointsInPolygons(munpols,
                    calls.Where(call => call.CallsTime.Hour == i).ToList());

                //counting the number of points
                foreach (var poly in PointsInPolygons)
                {
                    munpols[poly.Key].CallsCount.Add(poly.Value.Count);
                }
            }

            return calls;
        }

        public void GetLifeFactorsInDataPoints(Dictionary<string, MunPolygon> munpols)
        {
            foreach (var poly in munpols.Values)
            {
                for (int i = 0; i < 24; i++)
                {
                    poly.EmcWorkFactors.Add(((double) poly.CallsCount[i]/poly.PeopleDensity[i])*
                                            poly.AverageHospitalTimes[i]);
                }
            }
        }

        public Dictionary<string, List<PolyGisLayer>> GenerateMainPolyLayers()
        {
            var polyLayers = new Dictionary<string, List<PolyGisLayer>>();

            foreach (var munPolygon in dinamicPolygons.Values)
            {
                for (int j = 0; j < munPolygon.Points.Count; j++)
                {
                    if (munPolygon.Points[j].Count == 0) continue;

                    var polyCoords = new DVector2[10000];
                    for (int i = 0; i < munPolygon.Points[j].Count; i++)
                    {
                        GeoCoordinate cur_point = munPolygon.Points[j][i];
                        polyCoords[i] = new DVector2(DMathUtil.DegreesToRadians(cur_point.Longitude),
                            DMathUtil.DegreesToRadians(cur_point.Latitude));
                    }

                    var cont = PolyGisLayer.CreateFromContour(Game, polyCoords, Color.Red);
                    cont.Texture = Game.Content.Load<Texture2D>("grad - 100%"); //"palette_v3");
                    cont.PaletteTransparency = 0.35f;
                    //cont.PaletteValue = (float)(munPolygon.Lifefactor * 1000);

                    //0.4f + (0.1f*munPolygon.NormCoeff);
                    cont.IsVisible = false;
                    GisLayers.Add(cont);

                    //dinamicPolyLayers.Add(cont);
                    if (polyLayers.ContainsKey(munPolygon.Name))
                        polyLayers[munPolygon.Name].Add(cont);
                    else
                    {
                        polyLayers.Add(munPolygon.Name, new List<PolyGisLayer>() {cont});
                    }
                }
            }



            int pointscount = GetPoligonsPointsCount(dinamicPolygons.Values.ToList());

            var bounds = new LinesGisLayer(Game, 2*pointscount + 1, true);
            bounds.Flags = (int) (LinesGisLayer.LineFlags.DRAW_LINES);
            int curline_number = 0;
            //palette rgb
            //247,70,98
            //253,184,93
            //253,228,112
            //141,212,134
            //20,176,129


            foreach (var munPolygon in dinamicPolygons.Values.ToList())
            {
                for (int j = 0; j < munPolygon.Points.Count; j++)
                {
                    if (munPolygon.Points[j].Count == 0) continue;

                    for (int i = 0; i < munPolygon.Points[j].Count; i++)
                    {
                        GeoCoordinate cur_point = munPolygon.Points[j][i];
                        GeoCoordinate nextpoint = i == munPolygon.Points[j].Count - 1
                            ? munPolygon.Points[j][0]
                            : munPolygon.Points[j][i + 1];

                        Color clColor = Color.Aqua;
                        /*if (munPolygon.Lifefactor * 1000 >= 0.8) clColor = new Color(247, 70, 98);

                        else if (munPolygon.Lifefactor * 1000 >= 0.6) clColor = new Color(253, 184, 93);

                        else if (munPolygon.Lifefactor * 1000 >= 0.4) clColor = new Color(253, 228, 112);

                        else if (munPolygon.Lifefactor * 1000 >= 0.2) clColor = new Color(141, 212, 134);

                        else clColor = new Color(20, 176, 129);
                        */
                        bounds.PointsCpu[2*curline_number] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(cur_point.Longitude),
                            Lat = DMathUtil.DegreesToRadians(cur_point.Latitude),
                            Color = clColor, //new Color4(0.8f,0.8f,0.8f,1),//new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                            Tex0 = new Vector4(0.02f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width,
                        };

                        bounds.PointsCpu[2*curline_number + 1] = new Gis.GeoPoint
                        {
                            Lon = DMathUtil.DegreesToRadians(nextpoint.Longitude),
                            Lat = DMathUtil.DegreesToRadians(nextpoint.Latitude),
                            Color = clColor, //new Color4(0.8f, 0.8f, 0.8f, 1),//new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                            Tex0 = new Vector4(0.02f, 0.0f, 0.0f, 0.0f) // Tex0.X - half width
                        };

                        curline_number += 1;
                    }
                }
            }
            bounds.UpdatePointsBuffer();
            bounds.IsVisible = false;
            GisLayers.Add(bounds);

            dinamicLinesLayer = bounds;

            return polyLayers;
        }

    }



    public class EmcCall
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class MunPolygon
    {
        public List<List<GeoCoordinate>> Points { get; set; }
        public string Name { get; set; }
        public double Intensive { get; set; }
        public int NormCoeff { get; set; }
        public int NotNormCoeff { get; set; }
        public double Lifefactor { get; set; }

        public List<int> CallsCount { get; set; } = new List<int>();
        public List<double> PeopleDensity { get; set; } = new List<double>();
        public List<double> AverageHospitalTimes { get; set; } = new List<double>();
        public List<double> EmcWorkFactors { get; set; } = new List<double>();


        public MunPolygon()
        {
            Points = new List<List<GeoCoordinate>>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Quad;
using Pulse.MultiagentEngine.Map;

//using Pulse.Scenery.Cinema.Data.Preparers;
//using Pulse.Scenery.Cinema.Data.Readers;
//using Pulse.Scenery.Corridor.Data;
//using Pulse.Scenery.Corridor_T.Data;
//using Pulse.Scenery.MovingPlatform;
//using Pulse.Scenery.MovingPlatform.Evac;
//using Pulse.Scenery.Pulkovo.AirportObjects;
//using Pulse.Scenery.SeaStationRough;
//using Pulse.Scenery.Ship;
//using Pulse.Scenery.SubwayStation.Vasileostrovskaya;

namespace Pulse.GraphUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            const string inDir = @"C:\work\Pulse\ship_evacuation\Pulse.Model\InputData\Krestovsky\";
            const string inFile = inDir + @"scenery\map.json";
            const string outFile = inDir + @"scenery\map_qtg.json";
            var offset = new PulseVector2(0, 0);

            var getGraph = new Func<string, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]>, IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>>>(LoadKrestovskyGraphs);
            var writeGraph = new Action<string, string, IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>>, PulseVector2>(AddOrUpdateGraph);

            writeGraph(inFile, outFile, getGraph(inDir, GetInitBfsQuadrantByRandomPortal), offset);
        }

        private static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadKrestovskyGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
        {
            var pjrCfg = new PulseJsonReaderConfig
            {
                Levels = true,
                Way = false,
                Obstacle = true,
                Poi = false,
                Portal = true,
                Staff = false,
                Zone = false,
                Offset = new PulseVector2(0, 0)
            };
            var cbr = new PulseJsonReader("Krestovsky json reader", pjrCfg, inputDir);
            cbr.Initialize();

            var pl = new Dictionary<string, Func<ILevelPortal, IExternalPortal>>
            {
//                {"metro", portal => new ExternalPortal(portal)},
                {"NK_Out", portal => new ExternalPortal(portal)}
            };

            var sspp = new SimplePortalBuilder(cbr.PulseObject, pl);
            sspp.Initialize();

            var sslp = new SimpleLevelsBuilder(cbr.PulseObject, sspp.Portals, null);
            sslp.Initialize();

            var maxx = sslp.Levels.First().Value.Obstacles.Max(o => o.Max(p => p.X));
            var maxy = sslp.Levels.First().Value.Obstacles.Max(o => o.Max(p => p.Y));

            var minx = sslp.Levels.First().Value.Obstacles.Min(o => o.Min(p => p.X));
            var miny = sslp.Levels.First().Value.Obstacles.Min(o => o.Min(p => p.Y));

            var qtConfig = new QuadTreeConfig
            {
                BottomLeftCorner = new PulseVector2(-1600, -1600),
                Size = 3000,
                MinSize = 1,
                MaxSize = 10
            };
              
            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc, qtConfig);
            qtgl.Initialize();

            return qtgl.LeveledSubGraphs;
        }

//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadCinemaSubGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = true,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = true,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("Cinema json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//            var cprtp = new CinemaPortalBuilder(building, new MovieSession[0]);
//            cprtp.Initialize();
//            var clp = new CinemaLevelsPreparer(building, cprtp.Portals, null);
//            clp.Initialize();
//            var qtgl = new MultilevelQuadTreeGraphLoader(clp.Levels, getInitBfsQuadrantFunc);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadCorridorGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = false,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("Corridor json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new CorridorPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new CorridorLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadCorridorTGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = false,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("Corridor T json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new CorridorTPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new CorridorTLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadSeaStationSubGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = false,
//                Offset = new Coords(200, 100)
//            };
//            var cbr = new PulseJsonReader("Sea Station json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new SeaStationRoughPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new SeaStationRoughLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadSubwayStationSubGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = true,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("Subway Station json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new VasileostrovskayaPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new VasileostrovskayaLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadMovingPLatformEvacGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = true,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("MP json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new EvacMovingPlatformPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new MovingPlatformLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtConfig = new QuadTreeConfig
//            {
//                BottomLeftCorner = new Coords(150, 70),
//                Size = 30,
//                MinSize = 0.25,
//                MaxSize = 0.5
//            };
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc, qtConfig);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadMovingPLatformEvacShipGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = true,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("MP json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new Pulse.Scenery.MovingPlatform.EvacShip.EvacShipMovingPlatformPortalPreparer(building);
//            sspp.Initialize();
//
//            var sslp = new MovingPlatformLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtConfig = new QuadTreeConfig
//            {
//                BottomLeftCorner = new Coords(0, 0),
//                Size = 20,
//                MinSize = 0.25,
//                MaxSize = 0.5
//            };
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc, qtConfig);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadMovingPLatformSquareGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = true,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("MP json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sspp = new Pulse.Scenery.MovingPlatform.Square.SquareMovingPlatformPortalPreparer(building, "type_5");
//            sspp.Initialize();
//
//            var sslp = new MovingPlatformLevelsPreparer(cbr.PulseObject, sspp.Portals, null);
//            sslp.Initialize();
//
//            var qtConfig = new QuadTreeConfig
//            {
//                BottomLeftCorner = new Coords(-10, -10),
//                Size = 20,
//                MinSize = 0.25,
//                MaxSize = 0.5
//            };
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(sslp.Levels, getInitBfsQuadrantFunc, qtConfig);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//        }
//
//        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadShipSubGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
//        {
//            var pjrCfg = new PulseJsonReaderConfig
//            {
//                Levels = true,
//                Way = false,
//                Obstacle = true,
//                Poi = true,
//                Portal = true,
//                Staff = false,
//                Zone = false,
//                Offset = new Coords(0, 0)
//            };
//            var cbr = new PulseJsonReader("Subway Station json reader", pjrCfg, inputDir);
//            cbr.Initialize();
//
//            var building = cbr.PulseObject;
//
//            var sprtp = new ShipPortalPreparer(building);
//            sprtp.Initialize();
//
//            var plprtp = new PulkovoLocalPortalPreparer(building);
//            plprtp.Initialize();
//
//            var plp = new PulkovoLevelsPreparer(building, sprtp.Portals, plprtp.Portals);
//            plp.Initialize();
//
//            var qtConfig = new QuadTreeConfig
//            {
//                BottomLeftCorner = new Coords(-10, -10),
//                Size = 20,
//                MinSize = 0.25,
//                MaxSize = 0.5
//            };
//
//            var qtgl = new MultilevelQuadTreeGraphLoader(plp.Levels, getInitBfsQuadrantFunc, qtConfig);
//            qtgl.Initialize();
//
//            return qtgl.LeveledSubGraphs;
//            return null;
//        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantSeaStation(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.ExternalPortals.Select(p => p).Distinct());
            return new[] { poisQuadrants.RandomChoise().Value.Last() };
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantShip(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.PointsOfInterest.Select(p => p).Distinct());
            return new[] { poisQuadrants.RandomChoise().Value.Last() };
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantCinema(RasterizedQuadTree<RasterizeQuadTreeDataWrapper> treeLevelKvp, PulseLevel sbLevel)
        {
            var q = treeLevelKvp.QuadrantsChildless;
            var pois = sbLevel.PointsOfInterest.Select(p => p).Distinct();

            var pq = q.Where(cq =>
            {
                var data = cq.Data;
                if (data == null)
                    return false;

                var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                if (poiW == null)
                    return false;

                return pois.Any(poi => poiW.PointOfInterest == poi);
            }).ToList();

            var initQuadrantBfs = pq.Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();

            return new[] { initQuadrantBfs };
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantByRandomPoi(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.PointsOfInterest.Select(p => p).Distinct());
            return new[] {poisQuadrants.RandomChoise().Value.RandomChoise()};
        }



        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantByRandomPortal(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.ExternalPortals.Select(p => p).Distinct());
            return new[] { poisQuadrants.RandomChoise().Value.RandomChoise() };
        }

        public static IDictionary<IPointOfInterest, Quadrant<RasterizeQuadTreeDataWrapper>[]> GetPoisQuadrants(RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, IEnumerable<IPointOfInterest> pois)
        {
            var quadrants = levelTree.QuadrantsChildless;
            var poisQuadrants = new Dictionary<IPointOfInterest, Quadrant<RasterizeQuadTreeDataWrapper>[]>();

            foreach (var poi in pois)
            {
                var poiQuadrants = quadrants.Where(quadrant =>
                {
                    // 1. quadrant data required
                    var data = quadrant.Data;
                    if (data == null)
                        return false;

                    // 2. is quadrant poi typed quadrant?
                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    // 3. is quadrant belongs current poi?
                    return poiW.PointOfInterest == poi;
                }).ToArray();

                if (poiQuadrants.Length > 0)
                {
                    poisQuadrants[poi] = poiQuadrants;
                }
            }

            return poisQuadrants;
        }



        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantPulkovo(KeyValuePair<int, RasterizedQuadTree<RasterizeQuadTreeDataWrapper>> treeLevelKvp, PulseLevel sbLevel)
        {
            if (treeLevelKvp.Key == 3 || treeLevelKvp.Key == 4)
                return GetInitBfsQuadrantCinema(treeLevelKvp.Value, sbLevel);

            if (treeLevelKvp.Key == 1)
            {
                var poi1Id = "538";
                var poi2Id = "539";

                var q = treeLevelKvp.Value.QuadrantsChildless;
                var pois = sbLevel.PointsOfInterest.Where(poi => poi.ObjectId == poi1Id || poi.ObjectId == poi2Id);

                var pq = q.Where(cq =>
                {
                    var data = cq.Data;
                    if (data == null)
                        return false;

                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    return pois.Any(poi => poiW.PointOfInterest == poi);
                }).ToList();

                var initQuadrantBfs1 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi1Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();
                var initQuadrantBfs2 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi2Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();

                return new[] { initQuadrantBfs1, initQuadrantBfs2 };
            }

            if (treeLevelKvp.Key == 2)
            {
                var poi1Id = "1110";
                var port2Id = "1119";

                var q = treeLevelKvp.Value.QuadrantsChildless;
                var pois = sbLevel.PointsOfInterest.Where(poi => poi.ObjectId == poi1Id).Concat(sbLevel.ExternalPortals.Where(p => p.ObjectId == port2Id));

                var pq = q.Where(cq =>
                {
                    var data = cq.Data;
                    if (data == null)
                        return false;

                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    return pois.Any(poi => poiW.PointOfInterest == poi);
                }).ToList();

                var initQuadrantBfs1 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi1Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();
                var initQuadrantBfs2 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == port2Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();

                return new[] { initQuadrantBfs1, initQuadrantBfs2 };
            }

            throw new Exception(string.Format("unknowl level: {0}", treeLevelKvp.Key));
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantPulkovoa(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            if (level.Floor == 3 || level.Floor == 4)
            {
                var poisQuadrants = GetPoisQuadrants(levelTree, level.PointsOfInterest.Select(p => p).Distinct());
                return poisQuadrants.RandomChoise().Value;
            }

            if (level.Floor == 1)
            {
                var poi1Id = "538";
                var poi2Id = "539";

                var q = levelTree.QuadrantsChildless;
                var pois = level.PointsOfInterest.Where(poi => poi.ObjectId == poi1Id || poi.ObjectId == poi2Id);

                var pq = q.Where(cq =>
                {
                    var data = cq.Data;
                    if (data == null)
                        return false;

                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    return pois.Any(poi => poiW.PointOfInterest == poi);
                }).ToList();

                var initQuadrantBfs1 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi1Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();
                var initQuadrantBfs2 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi2Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();

                return new[] { initQuadrantBfs1, initQuadrantBfs2 };
            }

            if (level.Floor == 2)
            {
                var poi1Id = "1110";
                var port2Id = "1119";

                var q = levelTree.QuadrantsChildless;
                var pois = level.PointsOfInterest.Where(poi => poi.ObjectId == poi1Id).Concat(level.ExternalPortals.Where(p => p.ObjectId == port2Id));

                var pq = q.Where(cq =>
                {
                    var data = cq.Data;
                    if (data == null)
                        return false;

                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    return pois.Any(poi => poiW.PointOfInterest == poi);
                }).ToList();

                var initQuadrantBfs1 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == poi1Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();
                var initQuadrantBfs2 = pq.Where(d => (d.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest.ObjectId == port2Id).Where(d => d.Data != null && d.Data.State != QuadrantGraphNodeState.Obstacle).RandomChoise();

                return new[] { initQuadrantBfs1, initQuadrantBfs2 };
            }

            throw new Exception(string.Format("unknowl level: {0}", level.Floor));
        }


        

        public static IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadPulkovoSubGraphs(string inputDir, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
        {
            //            var r = new PulkovoBuildingReader(inputDir);
            //            r.Initialize();
            //            var building = r.pulseObject;
            //            
            //            var pprtp = new PulkovoPortalPreparer(building, new ArrivalFlight[0], new DepartureFlight[0], DateTime.Now);
            //            pprtp.Initialize();
            //
            //            var plprtp = new PulkovoLocalPortalPreparer(building);
            //            plprtp.Initialize();
            //
            //            var plp = new PulkovoLevelsPreparer(building, pprtp.Portals, plprtp.Portals);
            //            plp.Initialize();
            //
            //            var qtgl = new MultilevelQuadTreeGraphLoader(plp.Levels, getInitBfsQuadrantFunc);
            //            qtgl.Initialize();
            //
            //            return qtgl.LeveledSubGraphs;
            return null;
        }

        

        public static void AddOrUpdateGraph(string inputFile, string outPutFile,
            IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> leveledSubGraphs, PulseVector2 offset = default(PulseVector2))
        {
            var jBuilding = JObject.Parse(File.ReadAllText(inputFile));

            foreach (var subGraphKvp in leveledSubGraphs)
            {
                var level = subGraphKvp.Key;
                var graph = subGraphKvp.Value;

                var jLevel = jBuilding["levels"].First(l => l["level"].Value<int>() == level);
                
                // no json graph
                if (jLevel.Children<JProperty>().All(c => c.Name != "way"))
                {
                    var ea = new JArray();
                    var ep = new JProperty("edges", ea);

                    var va = new JArray();
                    var vp = new JProperty("vertices", va);

                    var tp = new JProperty("type", "graph");
                    
                    var wo = new JObject {tp, ep, vp};
                    var wp = new JProperty("way", wo);
                    
                    (jLevel as JObject).Add(wp);
                }

                var jsonVertices = jBuilding["levels"].First(l => l["level"].Value<int>() == level)["way"]["vertices"] as JArray;
                var jsonEdges = jBuilding["levels"].First(l => l["level"].Value<int>() == level)["way"]["edges"] as JArray;

                jsonVertices.Children().ToList().ForEach(c => c.Remove());
                jsonEdges.Children().ToList().ForEach(c => c.Remove());


                foreach (var vertex in graph.Vertices)
                {
                    jsonVertices.Add(new JObject(new JProperty("Id", vertex.Value.Id),
                        new JProperty("Point",
                            new JObject(new JProperty("x", vertex.Value.NodeData.Point.X - offset.X),
                                new JProperty("y", vertex.Value.NodeData.Point.Y - offset.Y)))));
                }

                foreach (var edge in graph.Edges)
                {
                    jsonEdges.Add(new JObject(
                        new JProperty("from", edge.Value.NodeFrom.Id),
                        new JProperty("to", edge.Value.NodeTo.Id)));
                }
            }

            using (var file = File.CreateText(outPutFile))
            using (var writer = new JsonTextWriter(file))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, jBuilding);
            }
        }
    }
}

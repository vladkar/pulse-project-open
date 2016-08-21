using System.Collections.Generic;
using System.IO;
using System.Linq;
using NavigField;
using Newtonsoft.Json.Linq;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Loaders
{
    public class PulseJsonReaderConfig
    {
        public bool Levels { set; get; }
        public bool Zone { set; get; }
        public bool Staff { set; get; }
        public bool Portal { set; get; }
        public bool Obstacle { set; get; }
        public bool Poi { set; get; }
        public bool Way { set; get; }
        public bool Navfield { set; get; }
        public PulseVector2 Offset { set; get; }
        public double SpaceMultiplier { get; set; }
        
        public PulseJsonReaderConfig()
        {
            Offset = new PulseVector2(0, 0);
            SpaceMultiplier = 1;
        }
    }

    public class PulseJsonReader : AbstractFileDataReader
    {
        public PulseObject PulseObject { set; get; }
        private IDictionary<string, PointOfInterestType> _poiTypes;
        private IDictionary<long, Vertex> _vertices = new Dictionary<long, Vertex>();
        public PulseJsonReaderConfig _config { set; get; }

        public PulseJsonReader(string name, PulseJsonReaderConfig config, string direPath, string file = "scenery\\map.json")
            : base(direPath)
        {
            _poiTypes = new Dictionary<string, PointOfInterestType>();
            _config = config;
            Name = name;
            DataFile = file;
        }

        protected override void LoadData()
        {
            var jPulseObject = JObject.Parse(File.ReadAllText(DataFullPath));

            PulseObject = LoadBuilding(jPulseObject);
        }

        private PulseObject LoadBuilding(JToken jPulseObject)
        {
            return new PulseObject
            {
                ObjectId = LoadId(jPulseObject),
                Name = jPulseObject["name"].Value<string>(),
                Description = jPulseObject["description"].Value<string>(),
                Levels = LoadLevels(jPulseObject["levels"]),
                Version = jPulseObject.Value<string>("version") ?? "",
                Navfields = LoadNavfields(jPulseObject["navfields"])
            };
        }

        private IDictionary<string, INavfieldCalc> LoadNavfields(JToken jNavfields)
        {
            if (_config.Navfield == false || jNavfields == null) return null;

            var navfieldstructure = new Dictionary<string, INavfieldCalc>();

            foreach (var nf in jNavfields)
            {
                int xSize = nf["field"]["size"]["xSize"].Value<int>();
                int ySize = nf["field"]["size"]["ySize"].Value<int>();

                var nfVar = new NavfieldCalc();
                var nfArr = nf["field"]["fieldArray"] as JArray;

                var jOffset = nf["field"]["offset"];
                nfVar.NF = new NavigationFieldSpace(xSize, ySize);
                nfVar.NF._offset.X = jOffset["X"].Value<double>();
                nfVar.NF._offset.Y = jOffset["Y"].Value<double>();
                nfVar.NF._toPulseVector2Multiplier = nf["field"]["toMeterMultiplier"].Value<double>();

                var nfGrid = new NavigationGridCell[xSize, ySize];
                var nfArrList = nfArr.Children().ToList();

                for (int i = 0; i < xSize; i++)
                    for (int j = 0; j < ySize; j++)
                    {
                        nfGrid[i, j] = new NavigationGridCell();
                        nfGrid[i, j].angle = nfArrList.ElementAt(i*ySize + j)["angle"].Value<double>();
                        nfGrid[i, j].velocity = nfArrList.ElementAt(i*ySize + j)["velocity"].Value<double>();
                    }

                nfVar.NF.NavigFieldArray = nfGrid;

                navfieldstructure.Add(nf["poiId"].Value<string>(), nfVar);
            }

            return navfieldstructure;
        }
        
        private IDictionary<int, Level> LoadLevels(JToken jLevels)
        {
            if (!_config.Levels) return null;
            return jLevels.ToDictionary(jLevel => jLevel["level"].Value<int>(), LoadLevel);
        }

        private Level LoadLevel(JToken jLevel)
        {
            return new Level
            {
                ObjectId = LoadId(jLevel),
                LevelFloor = jLevel["level"].Value<int>(),
                Obstacles = LoadObstacles(jLevel["obstacles"]),
                Edges = LoadEdges(jLevel["way"]),
                Vertices = _vertices,
                PointsOfInterest = LoadPointsOfInterest(jLevel["poi"]),
                Portals = LoadPortals(jLevel["portals"]),
                Zones = LoadZones(jLevel["zones"]),
                StaffPoints = LoadStaffPoints(jLevel["staff"])
            };
        }

        private ICollection<StaffPoint> LoadStaffPoints(JToken jStaffPoints)
        {
            if (!_config.Staff) return null;
            return jStaffPoints.Select(LoadStaffPoint).ToList();
        }

        private ICollection<Zone> LoadZones(JToken jZones)
        {
            if (!_config.Zone) return null;
            return jZones.Select(LoadZone).ToList();
        }

        private ICollection<LocalPortal> LoadPortals(JToken jPortals)
        {
            if (!_config.Portal) return null;
            return jPortals.Select(LoadPortal).ToList();
        }

        private ICollection<IPointOfInterest> LoadPointsOfInterest(JToken jPointsOfInterest)
        {
            if (!_config.Poi) return null;
            if (jPointsOfInterest == null) return new List<IPointOfInterest>();
            return jPointsOfInterest.Select(LoadPointOfInterest).ToList();
        }

        private ICollection<Obstacle> LoadObstacles(JToken jObstacles)
        {
            if (!_config.Obstacle) return null;
            return jObstacles.Select(LoadObstacle).ToList();
        }

        private ICollection<Edge> LoadEdges(JToken jGraph)
        {
            if (!_config.Way) return null;

            _vertices = LoadVertices(jGraph["vertices"]);
            return jGraph["edges"].Select(LoadEdge).ToList();
        }

        private IDictionary<long, Vertex> LoadVertices(JToken jGraph)
        {
            return jGraph.Select(LoadVertex).ToList().ToDictionary(item => item.Id);
        }

        private StaffPoint LoadStaffPoint(JToken jStaffPoint)
        {
            return new StaffPoint
            {
                Point = LoadPoint(jStaffPoint["point"]),
                Level = jStaffPoint.Parent.Parent.Parent["level"].Value<int>()
            };
        }

        private Zone LoadZone(JToken jZone)
        {
            return new Zone
            {
                ObjectId = LoadId(jZone),
                Name = jZone["type"].Value<string>(),
                Polygon = LoadPolygon(jZone)
            };
        }

        private IPointOfInterest LoadPointOfInterest(JToken jPointOfInterest)
        {
            return new PointOfInterest
            {
                ObjectId = LoadId(jPointOfInterest),
                Polygon = LoadPolygon(jPointOfInterest).ToArray(),
                Types = GetTypes(jPointOfInterest["type"].Value<string>()),
                Level = GetPoiLevel(jPointOfInterest)
            };
        }

        private int GetPoiLevel(JToken jPointOfInterest)
        {
            return jPointOfInterest.Parent.Parent.Parent["level"].Value<int>();
        }

        private IList<PointOfInterestType> GetTypes(string value)
        {
            if (!_poiTypes.ContainsKey(value))
                _poiTypes.Add(value, new PointOfInterestType { Id = IdUtil.NextRandomId(), Name = value });
            return new List<PointOfInterestType> { _poiTypes[value] };
        }

        private LocalPortal LoadPortal(JToken jPortal)
        {
            return new LocalPortal
            {
                ObjectId = LoadId(jPortal),
                Polygon = LoadPolygon(jPortal),
                TargetLink = jPortal["link"].Value<string>(),
                Level = jPortal.Parent.Parent.Parent["level"].Value<int>(),
                Exitable = jPortal["exit"].Value<bool>(),
                Enterable = jPortal["enter"].Value<bool>()
            };
        }

        private Obstacle LoadObstacle(JToken jObstacle)
        {
            return new Obstacle
            {
                ObjectId = LoadId(jObstacle),
                Polygon = LoadPolygon(jObstacle)
            };
        }

        private Edge LoadEdge(JToken jGraph)
        {
            return new Edge
            {
                From = _vertices[LoadVertexFrom(jGraph)],
                To = _vertices[LoadVertexTo(jGraph)]
            };
        }

        private Vertex LoadVertex(JToken jGraph)
        {
            return new Vertex
            {
                Id = LoadVertexId(jGraph),
                Point = LoadPoint(jGraph["Point"])
            };
        }

        private PulseVector2[] LoadPolygon(JToken jObject)
        {
            return jObject["polygon"].Select(LoadPoint).ToArray();
        }

        private PulseVector2 LoadPoint(JToken jObject)
        {
            var point = new PulseVector2
            {
                X = jObject["x"].Value<double>() * _config.SpaceMultiplier,
                Y = jObject["y"].Value<double>() * _config.SpaceMultiplier
            };

            return point + _config.Offset;
        }

        private string LoadId(JToken jObject)
        {
            return jObject["id"].Value<string>();
        }

        private int LoadVertexId(JToken jObject)
        {
            return jObject["Id"].Value<int>();
        }

        private int LoadVertexFrom(JToken jObject)
        {
            return jObject["from"].Value<int>();
        }

        private int LoadVertexTo(JToken jObject)
        {
            return jObject["to"].Value<int>();
        }
    }
}
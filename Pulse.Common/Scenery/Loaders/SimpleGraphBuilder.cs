using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Graph;

namespace Pulse.Common.Scenery.Loaders
{
    public class SimpleGraphBuilder : AbstractDataBroker
    {
        private PulseObject _pulseObject;
        private Dictionary<int, PulseLevel> _levels;

        public RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> Graph { get; set; }

        public SimpleGraphBuilder(Dictionary<int, PulseLevel> levels, PulseObject pulseObject)
        {
            _levels = levels;
            _pulseObject = pulseObject;
        }

        protected override void LoadData()
        {
            Graph = LoadGraph(_levels, _pulseObject);
        }


        private RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> LoadGraph(Dictionary<int, PulseLevel> sbLevels, PulseObject pulseObject)
        {
            var rawLevel = pulseObject.Levels.First().Value;

            var sw = new Stopwatch();
            Console.WriteLine("Start");
            sw.Start();


            var nodes = rawLevel.Vertices.Select(v => new Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>
            {
                Id = v.Value.Id,
                NodeData = new VertexDataPseudo3D
                {
                    Floor = rawLevel.LevelFloor,
                    Point = v.Value.Point
                }
            }).ToList();
            
            var comparer = new FuncComparer<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>((v1, v2) => v1.Id.CompareTo(v2.Id));
            nodes.Sort(comparer);

            var edges = rawLevel.Edges.Select(e =>
            {
                var fakeVertex = new Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>();

                fakeVertex.Id = e.From.Id;
                var fromI = nodes.BinarySearch(fakeVertex, comparer);
                var nodeFrom = nodes[fromI];

                fakeVertex.Id = e.To.Id;
                var toI = nodes.BinarySearch(fakeVertex, comparer);
                var nodeTo = nodes[toI];

                var edge = new Edge<VertexDataPseudo3D, EdgeDataPseudo3D>
                {
                    NodeFrom = nodeFrom,
                    NodeTo = nodeTo,
                    EdgeData = new EdgeDataPseudo3D { Weight = nodeFrom.NodeData.Point.DistanceTo(nodeTo.NodeData.Point) }
                };

                nodeFrom.Edges.Add(edge);
                nodeTo.Edges.Add(edge);

                return edge;
            });

            var graph = new RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>
            {
                Vertices = nodes.ToDictionary(v => v.Id, v => v),
                Edges = edges.ToDictionary(e => e.Id, e => e)
            };

            sw.Stop();
            Console.WriteLine("graph_load_timer:" + sw.Elapsed);

            
            graph.BuildKdTree();
            graph.AddPoisToGraph(sbLevels[rawLevel.LevelFloor].PointsOfInterest);
            graph.AddPoisToGraph(sbLevels[rawLevel.LevelFloor].ExternalPortals);
            graph.BuildKdTree();

            return graph;
        }
    }
}
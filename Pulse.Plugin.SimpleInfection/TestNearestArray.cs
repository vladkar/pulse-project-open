using System.Collections.Generic;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Body;

namespace Pulse.Plugin.SimpleInfection
{
    public class TestNearestArray
    {
        public HashSet<SimpleInfectionPluginAgent>[,] Grid { set; get; }
        private double _nodesize;

        public TestNearestArray(PulseVector2 min, PulseVector2 max, double nodeSize)
        {
            _nodesize = nodeSize;

            var x = (int)((max.X - min.X) / _nodesize);
            var y = (int)((max.Y - min.Y) / _nodesize);

            Grid = new HashSet<SimpleInfectionPluginAgent>[x, y];

//            for (var i = 0; i < Grid.GetLength(0); i++)
//                for (var j = 0; j < Grid.GetLength(1); j++)
//                    Grid[i, j] = new HashSet<SimpleInfectionPluginAgent>();
        }

        public IList<SimpleInfectionPluginAgent> GetNearest(SimpleInfectionPluginAgent current)
        {
            var nearest = new List<SimpleInfectionPluginAgent>();
            var ax = (int)(current.Agent.Point.X / _nodesize);
            var ay = (int)(current.Agent.Point.Y / _nodesize);

            nearest.AddRange(Grid[ax, ay]);

            if (Grid[ax, ay - 1] != null)
                nearest.AddRange(Grid[ax, ay - 1]);

            if (Grid[ax, ay + 1] != null)
                nearest.AddRange(Grid[ax, ay + 1]);

            if (Grid[ax - 1, ay] != null)
                nearest.AddRange(Grid[ax - 1, ay]);

            if (Grid[ax - 1, ay - 1] != null)
                nearest.AddRange(Grid[ax - 1, ay - 1]);

            if (Grid[ax - 1, ay + 1] != null)
                nearest.AddRange(Grid[ax - 1, ay + 1]);

            if (Grid[ax + 1, ay] != null)
                nearest.AddRange(Grid[ax + 1, ay]);

            if (Grid[ax + 1, ay - 1] != null)
                nearest.AddRange(Grid[ax + 1, ay - 1]);

            if (Grid[ax + 1, ay + 1] != null)
                nearest.AddRange(Grid[ax + 1, ay + 1]);

            return nearest;
        }
    }
}

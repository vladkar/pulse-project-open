using System;
using System.Collections.Generic;
using System.Linq;
using City.Snapshot;
using City.Snapshot.Infection;
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
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Infection;


namespace City.ControlsClient.DomainClient
{
    public class PulseMicroModelControl : AbstractPulseAgentControl
    {
        protected ICommandSnapshot _currentCommand = null;

        protected GeoCartesUtil _geoutil;
        protected PulseMicroModelDeveloperView devview;
        protected PulseMicroModelBeautyView beautyview;

        protected IList<IPulseAgentData> Agents { set; get; }
        public DateTime ModelTime { set; get; }
        protected IList<BaseInfectionStage.InfectionStates> AgentsInfected { set; get; }
        protected NavfieldSnapshotExtension Navfield { set; get; }

        protected override void InitializeControl()
        {
            devview		= new PulseMicroModelDeveloperView(this);
            beautyview	= new PulseMicroModelBeautyView(this);

            Views[1] = devview;
            Views[2] = beautyview;
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            base.LoadControl(controlInfo);
            AgentsInfected = new BaseInfectionStage.InfectionStates[0];
//            _geoutil = new GeoCartesUtil(PulseMap.MapInfo.MinGeo, PulseMap.MapInfo.MetersPerMapUnit);
//            
//            var obstacles = PulseMap.Levels.Values.First().Obstacles;
//            var pois = PulseMap.Levels.Values.First().PointsOfInterest;
//            var portals = PulseMap.Levels.Values.First().ExternalPortals;

            //render map static shit
        }

        private bool _nfFlaf = false;

        protected override void UpdateControl(GameTime gameTime)
        {
            var s = CurrentSnapShot as PulseSnapshot;
            if (s != null)
            {
                Agents = s.Agents;
                ModelTime = s.Time;
            }


			if (s.Extensions.Count > 0)
            {
                var nfext = s.Extensions.Values.First() as NavfieldSnapshotExtension;
                if (nfext != null && _nfFlaf == false)
                {
                    if (nfext.Grid.Length > 3)
                        _nfFlaf = true;
                    Navfield = nfext;
                }

                var iext = s.Extensions.Values.First() as InfectionSnapshotExtension;
                if (iext != null)
                {
                    AgentsInfected = iext.AgentsInfection.Select(i => (BaseInfectionStage.InfectionStates) i).ToList();
                }
            }

            //DenseFieldTest();

        }

        public PulseMapData GetMap()
        {
            return PulseMap;
        }

        public IList<IPulseAgentData> GetAgents()
        {
            return Agents;
        }

        public IList<BaseInfectionStage.InfectionStates> GetInfectedAgents()
        {
            return AgentsInfected;
        }

        public NavfieldSnapshotExtension GetNavfield()
        {
            return Navfield;
        }

        public override void UnloadControl()
        {
        }

        private bool corrGridInit = false;
        //private double[,] corrGrid;
        //private IDictionary<int, double[,]> corrGrid2;
        //private PulseVector2 corrGridOffset;

        private IDictionary<int, GridWrapper<CorrData>> corrGrid;
        public IDictionary<int, GridWrapper<List<CorrData>>> CorrData { set; get; }

        public void DenseFieldTest()
        {
            if (!corrGridInit)
            {
                if (PulseMap.Levels.Count > 0)
                {
                    corrGrid = new Dictionary<int, GridWrapper<CorrData>>();
                    CorrData = new Dictionary<int, GridWrapper<List<CorrData>>>();

                    foreach (var lvl in PulseMap.Levels)
                    {
                        var obstacles = lvl.Value.Obstacles;

                        var maxx = obstacles.Max(o => o.Max(p => p.X));
                        var maxy = obstacles.Max(o => o.Max(p => p.Y));

                        var minx = obstacles.Min(o => o.Min(p => p.X));
                        var miny = obstacles.Min(o => o.Min(p => p.Y));

                        corrGrid[lvl.Key] = new GridWrapper<CorrData>(minx, maxx, miny, maxy, 2);
                        CorrData[lvl.Key] = new GridWrapper<List<CorrData>>(minx, maxx, miny, maxy, 2);
                        CorrData[lvl.Key].PopulateGrid((() => new List<CorrData>()));
                    }

                    corrGridInit = true;
                }
            }
            if (corrGridInit)
            {
                foreach (var lvlGrid in corrGrid)
                {
                    lvlGrid.Value.PopulateGrid(() => new CorrData());
                }

                foreach (var agent in Agents)
                {
                    corrGrid[agent.Level].GetCell(agent.X, agent.Y).Count += 1;                    
                }

                var arrIn = corrGrid.Values.First().Grid;
                var arrOut = CorrData.Values.First().Grid;
                int rowLength = arrIn.GetLength(0);
                int colLength = arrIn.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < colLength; j++)
                    {
                        arrOut[i, j].Add(arrIn[i, j]);
                    }
                }
            }
        }

        public override string UserInfo()
        {
            return "";
        }
    }

    public class GridWrapper<T>
    {
        public T[,] Grid { set; get; }
        public PulseVector2 Offset { set; get; }
        public double CellSize { set; get; }

        //public GridWrapper()
        //{
         //   CellSize = 1;
        //}

        public GridWrapper(PulseVector2 min, PulseVector2 max, double cellSize = 1) : this(min.X, min.Y, max.X, max.Y, cellSize)
        {
        }

        public GridWrapper(double minx, double maxx, double miny, double maxy, double cellSize = 1)
        {
            CellSize = cellSize;

            var gridSizeX = (int)((maxx - minx) / CellSize + CellSize * 2);
            var gridSizeY = (int)((maxy - miny) / CellSize + CellSize * 2);

            Grid = new T[gridSizeX, gridSizeY];
            Offset = new PulseVector2(minx / CellSize + CellSize, miny / CellSize + CellSize);
        }

        public T GetCell(PulseVector2 p)
        {
            return GetCell(p.X, p.Y);
        }

        public PulseVector2 GetCoordsByIndex(int x, int y)
        {
            return new PulseVector2(x*CellSize + Offset.X - CellSize, y * CellSize + Offset.Y - CellSize);
        }

        public T GetCell(double x, double y)
        {
            return Grid[(int)((x - Offset.X) / CellSize + CellSize), (int)((y - Offset.Y) / CellSize + CellSize)];
        }

        public void PopulateGrid(Func<T> defaultVal)
        {
            int rowLength = Grid.GetLength(0);
            int colLength = Grid.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Grid[i, j] = defaultVal();
                }
            }
        }
    }

    public class CorrData
    {
        public double Press { set; get; }
        public double Count { set; get; }
    }
}

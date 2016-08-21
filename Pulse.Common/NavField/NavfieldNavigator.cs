using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;
using KDTree;
using NavigField;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;

namespace Pulse.Common.NavField
{
    public class NavFieldTemp
    {
        
    }

    public abstract class NavfieldNavigator : IPseudo3DAgentNavigator
    {
        public abstract IList<ITravelPath> GeneratePath(PulseVector2 start, PulseVector2 end, int levelStart, int levelEnd);
        public abstract PulseVector2 Getvelocity(PulseVector2 pos, PulseVector2 dest, int level);
    }


    //TODO current initialization is bad: 1. add static navfield register 2. if it was added, skip part of init
    public class PoiTreeNavfieldNavigator : NavfieldNavigator
    {
        public static IList<IPointOfInterest> Pois { get; protected set; }
        public static IDictionary<int, KDTree<IPointOfInterest>> PoiTree { get; protected set; }
        public static IDictionary<string, INavfieldCalc> NavFieldRegister { get; set; }

        private static bool _init = false;
        private static object _lock = new object();

        public delegate void OnNavfieldCalcCompleteDelegate(IPointOfInterest poi, NavfieldCalc c);
        public static event OnNavfieldCalcCompleteDelegate OnNavfieldCalcComplete;

        protected static void  OnNavfieldCalcCompleteInvoke(IPointOfInterest poi, NavfieldCalc c)
        {
            var handler = OnNavfieldCalcComplete;
            if (handler != null) handler(poi, c);
        }

        public PoiTreeNavfieldNavigator(PulseScenery s)
        {
            if (_init) return;
            lock (_lock)
            {
                Initialize(s);
            }
        }

        private static void Initialize(PulseScenery s)
        {
            PoiTree = new Dictionary<int, KDTree<IPointOfInterest>>();
            var allpois = new List<IPointOfInterest>();
            foreach (var level in s.Levels)
            {
                allpois.AddRange(level.Value.PointsOfInterest);
                allpois.AddRange(level.Value.ExternalPortals);
                allpois.AddRange(level.Value.LevelPortals);
                PoiTree[level.Key] = new KDTree<IPointOfInterest>(2);
            }

//            Pois = s.Levels.Values.s.PointsOfInterest.Concat(s.Levels.Values.First().ExternalPortals).ToList();
            Pois = allpois;

            foreach (var poi in Pois)
            {
                PoiTree[poi.Level].AddPoint(new double[] {poi.Point.X, poi.Point.Y}, poi);
            }

            if (NavFieldRegister != null && NavFieldRegister.Count > 0)
            {
                _init = true;
                return;
            }

            NavFieldRegister = new ConcurrentDictionary<string, INavfieldCalc>();
            var plist = new ConcurrentDictionary<string, IPointOfInterest>();
//                                    foreach (var poi in Pois)

            var completed = new ConcurrentDictionary<string, IPointOfInterest>();

            Parallel.ForEach(Pois, (poi) =>
            {

                PulseVector2 trgp;
                trgp = ClipperUtil.GetCentroid(poi.Polygon);

                plist[poi.ObjectId] = poi;

                Console.Out.WriteLine(poi.ObjectId + " start point " + trgp);
                var calc = new NavfieldCalc();
                var point =
                    s.TypedPointsOfInterest.Values.SelectMany(p => p).Where(p => p.Level == poi.Level).First().Point;
                calc.Load(0.25, s.Levels[poi.Level].Obstacles, new[] {point});

                var path = new PulseVector2[0];
                //                PulseVector2[] path = new PulseVector2[5];
                ////                path[0] = new PulseVector2(45, 25);
                //                path[0] = new PulseVector2(25, 36);
                //                path[1] = new PulseVector2(25, 39);
                //                path[2] = new PulseVector2(40, 39);
                //                path[3] = new PulseVector2(40, 35);
                //                path[4] = new PulseVector2(35, 35);

                calc.CalcFieldForOneAim(trgp, 6);
                //                calc.CalculateField(new PulseVector2(35, 35), path, 0);
                OnNavfieldCalcCompleteInvoke(poi, calc);
                NavFieldRegister[poi.ObjectId] = calc; // to poib, do it parallel
                Console.Out.WriteLine(poi.ObjectId + " end");

                IPointOfInterest ppp = null;
                plist.TryRemove(poi.ObjectId, out ppp);

                completed[poi.ObjectId] = poi;

                var count = Pois.Count(p => !completed.ContainsKey(p.ObjectId));

                Console.Out.WriteLine("------------------------");
                Console.Out.WriteLine($"Total count: {count}");


                foreach (var pp in plist)
                {
                    Console.Out.Write(pp.Value.ObjectId + " ");
                }

                Console.Out.WriteLine();

            }
                );

            _init = true;
        }

        public override PulseVector2 Getvelocity(PulseVector2 pos, PulseVector2 dest, int level)
        {
            var ePoiIterator = PoiTree[level].NearestNeighbors(new double[] { dest.X, dest.Y }, 1);
            while (ePoiIterator.MoveNext())
            {
                var ePoi = ePoiIterator.Current;
                var f = NavFieldRegister[ePoi.ObjectId];
                return f.GetVelocity(pos);
            }

            return new PulseVector2(0, 0);
        }

        
        public override IList<ITravelPath> GeneratePath(PulseVector2 start, PulseVector2 end, int levelStart, int levelEnd)
        {
//            var ePoi = PoiTree.NearestNeighbors(new[] { end.X, end.Y }, 1).Current;
//
//            lock (_lock)
//            {
//                if (!NavFieldRegister.ContainsKey(ePoi.ObjectId))
//                {
//                    var calc = new NavfieldCalc();
//                    calc.Load(0.5, s.Levels.First().Value.Obstacles, new[] { new PulseVector2(30, 30) });
//                    calc.CalcFieldForOneAim(ePoi.Point, 30, 5);
//                    NavFieldRegister[ePoi.ObjectId] = calc;
//                }
//            }
//            throw new System.NotImplementedException();

            return new []{new NavFieldTravelPath()};
        }
    }
}

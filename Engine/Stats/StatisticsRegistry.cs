using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.MultiagentEngine.Stats
{
    public class StatisticsRegistry : IStatisticsRegistry
    {
        private readonly List<IStatistics> _statisticses = new List<IStatistics>();
        private readonly SimulationEngine _engine;

        public void Update()
        {
            // parallel update
            _statisticses.AsParallel().ForAll(delegate(IStatistics statistics)
                                                  {
                                                      if(statistics is IAgentsStatistics)
                                                      {
                                                          ((IAgentsStatistics)statistics).Update(_engine.World.Agents, _engine.World.Time);
                                                      }else if(statistics is IMapStatistics)
                                                      {
                                                          ((IMapStatistics) statistics).Update(_engine.World.Map,
                                                                                               _engine.World.Time);
                                                      }else if(statistics is IWorldStatistics)
                                                      {
                                                          ((IWorldStatistics) statistics).Update(_engine.World);
                                                      }else if(statistics is IEngineStatistics)
                                                      {
                                                          ((IEngineStatistics) statistics).Update(_engine);
                                                      }
                                                      // ignore final stats
                                                  });

//            Update<IEngineStatistics>(x=>x.Update(_engine));
//            Update<IWorldStatistics>(x=>x.Update(_engine.World));
//            Update<IMapStatistics>(x => x.Update(_engine.World.Map, _engine.World.Time));
//            Update<IAgentsStatistics>(x => x.Update(_engine.World.Agents, _engine.World.Time));
        }

        private void Update<T>(Action<T> action) where T : IStatistics
        {
            foreach (var r in _statisticses.OfType<T>())
            {
                action(r);
            }
        }


        public T GetStatistics<T>() where T : IStatistics
        {
            return _statisticses.OfType<T>().Single(); //TODO check
        }

        public void Add(IStatistics statistics)
        {
            _statisticses.Add(statistics);
        }

        public void Remove(IStatistics statistics)
        {
            _statisticses.Remove(statistics);
        }

        public void Finalization()
        {
            Update<IFinalStatistics>(statistics => statistics.Update(_engine));

            foreach (var statistic in _statisticses)
            {
                statistic.Finalization();
            }
        }

        public void Remove<T>() where T : IStatistics
        {
            _statisticses.Remove(_statisticses.OfType<T>().Single()); //todo check
        }

        public StatisticsRegistry(SimulationEngine engine)
        {
            _engine = engine;
        }

    }



}
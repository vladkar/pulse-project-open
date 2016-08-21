using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using MultiagentEngine.Engine;
using MultiagentEngine.Containers;
using TrControl;
using City.Snapshot;
using MultiagentEngine.Generation;
using Pulse.Common.Utils;
using System.Diagnostics;
using City.Snapshot.Snapshot;
using City.Snapshot.TrafficAgent;
using SimpleTraffic;
using DistributedTraffic;
using DistributedTraffic.Emergency;

namespace City.ControlsServer
{
    public class TrafficServer : IPulseServer
    {
        private Task _cycleUpdateTask;
        private CancellationTokenSource _taskCancellationToken;

        #region instance spawning fields

        private TrafficEngineFactory _engineFactory;
        private TrafficConfig _config;
        private SimulationEngine _engine;

        #endregion
        /*
        private CoordType _coordSystem;
        private double _fps = 0; */

        private SimWorld _trafficWorld;
        private double _fps = 50;
        private bool IsStep = false;
        private int _iterVelocity = 0;
        private int _maxIntervelocity = 3;

        private int iteration_Count = 0;
        //      private GeoCartesUtil _gcu;
        //      private PulseMapsData _mapData;


        public ControlInfo ServerConfig { get; set; }

        private TrafficBinarySerializer _pba;
        
        private EnumElement _state;

        
        private ISnapshot _currentSnapShot;
        private byte[] _currentByteSnapShot;
        private IntervalSizedQueue<ISnapshot> _cache;

        public TrafficServer()
        {
            _state = PulseRunnerState.NotInitialized;
        }
        
        public void Initialize(PulseMasterServer pulseMasterServer)
        {
            _state = PulseRunnerState.Initializing;

            if (_engineFactory == null)
                _engineFactory = new TrafficEngineFactory();

            _state = PulseRunnerState.Initialized;
        }

        public void LoadLevel(ControlInfo config)
        {
            ServerConfig = config;

            _state = PulseRunnerState.Loading;

            _config = _engineFactory.GetConfig(config.Scenario);
            _engine = _engineFactory.GetEngine(_config) as SimulationEngine;
            _pba = new TrafficBinarySerializer();
            _engine.Start();
            Thread.Sleep(1500);

            _taskCancellationToken = new CancellationTokenSource();
            var token = _taskCancellationToken.Token;
            _cycleUpdateTask = new Task(CycleAuto, token);
            //_cycleUpdateTask = new Task( () => { while (true) { Thread.Sleep(100); Log.Message("Model Step");} } );
            _cycleUpdateTask.Start();

            _trafficWorld = _engine.World as SimWorld;
////            _gcu = new GeoCartesUtil(_engine.ScenarioConfig.MapConfig.MinGeo, _engine.ScenarioConfig.MapConfig.MetersPerMapUnit);
            
            

           ItilializeCache();
////            Configure(_engine.ScenarioConfig);

            _state = PulseRunnerState.Running;
        }

        public void UnloadLevel()
        {
            _state = PulseRunnerState.Unloading;

            if (_cycleUpdateTask != null)
            {
                _state = PulseRunnerState.Paused;

                _taskCancellationToken.Cancel();
                _engine.Stop();
                Thread.Sleep(500);
                _state = PulseRunnerState.NotInitialized;
                _engine = null;
                _cycleUpdateTask = null;

                _currentSnapShot = null;
                _cache = null;

                GC.Collect();
            }

            _state = PulseRunnerState.Finished;
        }
        
        public ISnapshot Update()
        {
            return _currentSnapShot;
        }
        
        public void FeedCommand(string id, ICommandSnapshot userCommand)
        {
        }

        public string ServerInfo()
        {
            return _state.Value;
        }

        
        

        private void ItilializeCache()
        {
            _cache = new IntervalSizedQueue<ISnapshot>(0, 100);
            _currentSnapShot = new TrafficSnapshot();
            _currentByteSnapShot = new byte[0];

            /*
            _mapData = new PulseMapsData
            {
                Xmin = x,
                Ymin = y,
                ToMetersMultiplier = _pulseWorld.ScenarioConfig.MetersPerMapUnit.Value,
                ToSecondsMultiplier = _pulseWorld.ScenarioConfig.ToSecondsMultiplier.Value
            };
            */
        }
        
        private void CycleAuto()
        {
            var sw = new Stopwatch();
            while (true)
            {
                if (_state == PulseRunnerState.Running)
                {
                    var iterationBottom = GetIterationTime();
                    sw.Reset();
                    sw.Start();

                    UpdateCache();
                    MakeIteration();

                    sw.Stop();
                    var iterationTime = sw.ElapsedMilliseconds;
                    var elapsed = iterationBottom - iterationTime;
                    if (elapsed > 0)
                        Thread.Sleep((int)elapsed);
                }
            }
        }

        private Object locker = new Object();

        private void UpdateCache()
        {
            var intKey = (int)_engine.Info.Time * 10;

            var agents = _trafficWorld.Agents.OfType<SimpleTrafficAgent>();
            var agentsDto = agents.Select(a =>
            {
                return _pba.GetTrafficAgentDto(a);               
            }).ToList();

            var map = _trafficWorld.Map as ZonedTrafficMap;

            //EmergencyGenerationModel.EmergencyStats = EmergencyGenerationModel.EmergencyStats;
            
            var edgesDto = map.Edges.Values.Where(e => e.AllAgentsOnTheEdge.Count!= 0).Select(e =>
            {
                return _pba.GetTrafficEdgeDto(e);
            }).ToList();

            var emcAgents = _trafficWorld.Agents.OfType<SimpleTrafficAgent>()
                .Where(agent => agent.Mode == BehaviorMode.Emergency && agent.IsAlive).ToList();


            var emcPathsDto = _pba.GetTrafficEmcPathDto(emcAgents);


            

            var selectDto = new List<TrafficSelectData>();

            List<TrafficCallData> callsDto = new List<TrafficCallData>();
            if (_config.Scenario == "vasilevskiy" || _config.Scenario == "almazovskiy")
            {
                agentsDto = agents.Where(agent => agent.Mode == BehaviorMode.Emergency).Select(a =>
                 {
                     return _pba.GetTrafficAgentDto(a);
                 }).ToList();

                callsDto = EmergencyGenerator.AllCalls.Where(call => (call.CurState!= CallState.WaitingHospitalization 
                || call.CurState!= CallState.EMCIsService)&& (call.CurState != CallState.Finish || call.IsDraw)).Select(call =>
                {
                    return _pba.GetTrafficCallDto(call);
                }).ToList();

                if (_config.Scenario == "almazovskiy")
                {
                    if (EmergencyGenerator.AllCalls.Count >= 5)
                    {
                        Call almazcall = EmergencyGenerator.AllCalls[4];
                        if (almazcall.CurState == CallState.Service)
                        {
                            if (almazcall.Candidates != null)
                            {
                                if (almazcall.Candidates.Count != 0)
                                {
                                    foreach (var candidate in almazcall.Candidates)
                                    {
                                        selectDto.Add(_pba.GeTrafficSelectDto(almazcall.Patient_Node, candidate));
                                    }
                                }
                            }
                        }
                    }
                }


            }

            EmergencyGenerationModel.EmergencyStats.IterationCount = iteration_Count;
            iteration_Count += 1;

            lock (locker)
                _currentSnapShot = new TrafficSnapshot { Number = intKey, Agents = agentsDto , Edges = edgesDto,
                EmcPaths = emcPathsDto,Calls = callsDto , SelectLines = selectDto, emc_Stats = EmergencyGenerationModel.EmergencyStats};

//            lock (locker)
//                _currentByteSnapShot = _pba.SerializeSnapShot(_currentSnapShot);

            //if (!_cache.ContainsKey(intKey))
            //    _cache.Add(intKey, _currentSnapShot);

            //Console.WriteLine( _engine.World.Agents.Count);
            //            var 
        }

        private void MakeIteration()
        {
            
           /* if (IsStep)
            {
                _engine.DoStep();
                IsStep = false;
            }
            _iterVelocity += 1;
            if (_iterVelocity == _maxIntervelocity)
            {
                IsStep = true;
                _iterVelocity = 0;
            }
            */
            _engine.DoStep();
        }

        private double GetIterationTime()
        {
            if (_fps == 0) return 0;
            return 1 * 1000 / _fps; //ms
        } 
    }
}

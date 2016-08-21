using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Fusion;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.Model;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Map;
using Pulse.Scenery.HarsiddhiTemple;
using Pulse.Scenery.SubwayStation.Abstract.Plugin;

namespace City.ControlsServer
{
    public class PulseServerAgentEngineStuff
    {
        public PulseWorld PulseWorld { set; get; }
        public PulseScenarioConfig PulseConfig { set; get; }
        public PulseEngine Engine {set; get; }
    }

    public class PulseServer : IPulseServer
    {
        private Task _cycleUpdateTask;
        private CancellationTokenSource _taskCancellationToken;

        #region instance spawning fields

        private ModelFactoryReflection _engineFactory;
        private PulseScenarioConfig _pulseConfig;
        private PulseEngine _engine;

        #endregion

        private CoordType _coordSystem;
        private double _fps = 0;

        private PulseWorld _pulseWorld;
        private GeoCartesUtil _gcu;
        private PulseMapsData _mapData;

        private IPulseSnapshotControlSerializer _pba;

        private EnumElement _state;

        protected ISnapshot _currentSnapShot;
        //        private byte[] _currentByteSnapShot;
        private IntervalSizedQueue<ISnapshot> _cache;
        private FpsControlInfo _controlConfig;

        //
        private IDictionary<byte, IExchangeSnapshot> _currentMyExchangeSnapshot;
        public int Id { get; private set; }

        private Queue<IExchangeSnapshot> _currentExchangeSnapshot;

        private Object locker = new Object();
        private SnapshotUtil _snapshotExtensionUtil;
        private PulseMasterServer _masterServer;

        public PulseServer()
        {
            _state = PulseRunnerState.NotInitialized;
        }

        public PulseServer(int id)
        {
            Id = id;
        }

        //todo extract it
        #region runner part

        public ulong GetIterations()
        {
            return _engine.Counters.IterationNumber;
        }

        public IList<IPulseAgentData> GetAgents()
        {
            return (_currentSnapShot as PulseSnapshot).Agents;
        }

        public event OnStepDelegate OnStep;
        public delegate void OnStepDelegate();
        protected void OnStepInvoke()
        {
            var handler = OnStep;
            if (handler != null) handler();
        }

        #endregion

        public void Initialize(PulseMasterServer pulseMasterServer)
        {
            _state = PulseRunnerState.Initializing;

            _masterServer = pulseMasterServer;
            _masterServer.Atoms.Add(GlobalStrings.PulseCommandFlow);
            _masterServer.Atoms.Add(GlobalStrings.PulseCommandFps);
            _masterServer.Atoms.Add(GlobalStrings.PulseCommandSf);

            if (_engineFactory == null)
                _engineFactory = new ModelFactoryReflection();

            _state = PulseRunnerState.Initialized;
        }

        public void LoadLevel(ControlInfo config)
        {
            _state = PulseRunnerState.Loading;

            _controlConfig = config as FpsControlInfo;
            _fps = _controlConfig.Fps;
            _pulseConfig = _engineFactory.GetConfig(config.Scenario);

            _pba = new PulseAgentBinarySerializer(_controlConfig);
            _snapshotExtensionUtil = new SnapshotUtil(_controlConfig);


            //            _pulseConfig.MovementSystems = new BaseConfigField<IDictionary<string, string>>(new Dictionary<string, string> { { "pedestrian", "simplevelocity" } });
            //            _pulseConfig.MovementSystems = new BaseConfigField<IDictionary<string, string>>(new Dictionary<string, string> { { "pedestrian", "sf" } });

            _engine = _engineFactory.GetEngine(_pulseConfig, SimulationRunMode.StepByStep) as PulseEngine;
            _engine.Start();
            (_engine.World as PulseWorld).Id = Id;
            Thread.Sleep(100);

            _taskCancellationToken = new CancellationTokenSource();
            var token = _taskCancellationToken.Token;
            _cycleUpdateTask = new Task(CycleAuto, token);
            _cycleUpdateTask.Start();

            _pulseWorld = _engine.World as PulseWorld;
            _gcu = new GeoCartesUtil(_engine.ScenarioConfig.MapConfig.MinGeo, _engine.ScenarioConfig.MapConfig.MetersPerMapUnit);


            ItilializeCache();
            Configure(_engine.ScenarioConfig);
            _state = PulseRunnerState.Loaded;
            _state = PulseRunnerState.Running;
        }

        public void UnloadLevel()
        {
            _state = PulseRunnerState.Unloading;

            if (_cycleUpdateTask != null)
            {
                _state = PulseRunnerState.Paused;
                _engine.Stop();
                _taskCancellationToken.Cancel();

                while (_cycleUpdateTask.Status != TaskStatus.Canceled &&
                       _cycleUpdateTask.Status != TaskStatus.RanToCompletion)
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(100);

                GC.Collect();
            }

            _state = PulseRunnerState.Finished;
        }

        public ISnapshot Update()
        {
            return _currentSnapShot;
        }

        public IDictionary<byte, IExchangeSnapshot> GetExchangeCnaphot()
        {
            var s = _currentMyExchangeSnapshot;
            _currentMyExchangeSnapshot = new ConcurrentDictionary<byte, IExchangeSnapshot>();

            return s; // ?? new ConcurrentDictionary<byte, IExchangeSnapshot>();
        }

        public void FeedExchangeCnaphot(IExchangeSnapshot snapshot)
        {
            if (_currentExchangeSnapshot == null)
                _currentExchangeSnapshot = new Queue<IExchangeSnapshot>();

            _currentExchangeSnapshot.Enqueue(snapshot);

            //            foreach (var s in snapshot)
            //            {
            //                if (_currentExchangeSnapshot[s.Key] == null)
            //                    _currentExchangeSnapshot[s.Key] = new Queue<IExchangeSnapshot>();
            //
            //                _currentExchangeSnapshot[s.Key].Enqueue(s.Value);
            //            }
        }

        public void FeedCommand(string id, ICommandSnapshot userCommand)
        {
            var cmd = userCommand;

            if (cmd == null)
                return;

            switch (cmd.Command)
            {
                case "flow":
                    double flow;
                    if (Double.TryParse(userCommand.Args[0], out flow))
                    {
                        Console.WriteLine(flow);

                        SimpleIterationProbabilityAgentsGenerator.K = flow;
                        SimpleIterationProbabilityAgentsGroupGenerator.K = flow;
                    }

                    break;
                case "sf":
                    ICommand command = new Command
                    {
                        Args = cmd.Args,
                        Cmd = cmd.Command
                    };
                    _pulseWorld.Engine.World.Map.SetCommand(command);

                    break;

                case "simfps":
                    double fps;
                    if (Double.TryParse(userCommand.Args[0], out fps))
                    {
                        Console.WriteLine(fps);

                        _fps = fps;
                    }

                    break;

                default:
                    Log.Warning($"Unknown server command: {cmd.Command}");
                    break;
            }
        }

        public string ServerInfo()
        {
            return _state.Value;
        }

        public PulseServerAgentEngineStuff GetAgetnEngine()
        {
            return new PulseServerAgentEngineStuff
            {
                Engine = _engine,
                PulseConfig = _pulseConfig,
                PulseWorld = _pulseWorld
            };
        }


        private void Configure(PulseScenarioConfig scenarioConfig)
        {
            _coordSystem = scenarioConfig.PreferredCoordinates.Value == "geo" ? CoordType.Geo : CoordType.Map;
            // vis config
            // init offset
            // init zoom
        }

        private void ItilializeCache()
        {
            _cache = new IntervalSizedQueue<ISnapshot>(0, 100);
            _currentSnapShot = new PulseSnapshot();
            _currentMyExchangeSnapshot = new ConcurrentDictionary<byte, IExchangeSnapshot>();
            //            _currentByteSnapShot = new byte[0];

            var pulseMap = _pulseWorld.Map as PulseMap;

            var x = 0d;
            var y = 0d;

            if (_coordSystem == CoordType.Map)
            {
                x = pulseMap.MapConfig.Min.X;
                y = pulseMap.MapConfig.Min.Y;
            }
            else if (_coordSystem == CoordType.Geo)
            {
                x = pulseMap.MapConfig.MinGeo.Lon;
                y = pulseMap.MapConfig.MinGeo.Lat;
            }

            _mapData = new PulseMapsData
            {
                Xmin = x,
                Ymin = y,
                ToMetersMultiplier = _pulseWorld.ScenarioConfig.MetersPerMapUnit.Value,
                ToSecondsMultiplier = _pulseWorld.ScenarioConfig.ToSecondsMultiplier.Value
            };
        }

        private void CycleAuto()
        {
            var sw = new Stopwatch();
            while (true)
            {
                if (_taskCancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (_state == PulseRunnerState.Running)
                {
                    var iterationBottom = GetIterationTime();
                    sw.Reset();
                    sw.Start();

                    //lock (locker)
                    //{
                    UpdateCache();
                    MakeIteration();
                    OnStepInvoke();
                    //}

                    sw.Stop();
                    var iterationTime = sw.ElapsedMilliseconds;
                    var elapsed = iterationBottom - iterationTime;
                    if (elapsed > 0)
                        Thread.Sleep((int)elapsed);
                }
            }
        }


        protected virtual void UpdateCache()
        {
            var intKey = (int)_engine.Info.Time * 10;

            var guests = new List<IGuestAgent>();

            _pulseWorld.AddGuestAgents(guests);

            var agents = _pulseWorld.Agents.OfType<AbstractPulseAgent>();

            var travelers = _pulseWorld.GetTravelersAgents();

            _currentMyExchangeSnapshot = travelers.GroupBy(t => t.DestWorld)
                .ToDictionary(g => g.Key, g => new PulseExchangeSnapshot { Agents = g.ToList<IPulseAgentData>(), Number = intKey, OriginServer = (byte)Id, DestinationServer = g.Key } as IExchangeSnapshot);

            var agentsDto = agents.Select(a =>
            {
                var adto = new SfAgent(a);

                if (_coordSystem == CoordType.Geo)
                {
                    var gc = _gcu.GetGeoCoordsTuple(adto.X, adto.Y);
                    adto.X = gc.Item1;
                    adto.Y = gc.Item2;
                }

                return adto as IPulseAgentData;
            }).ToList();

            //            var timeSeconds = _engine.Info.Time*_pulseConfig.ToSecondsMultiplier.Value;
            var time = (_engine.World as PulseWorld).GeoTime.GeoTime;
            _currentSnapShot = new PulseSnapshot { Number = intKey, Agents = agentsDto, Extensions = _snapshotExtensionUtil.GetExtensions(_pulseWorld.Map as PulseMap), Time = time };



            //                _currentByteSnapShot = _pba.SerializeSnapShot(_currentSnapShot);

            //if (!_cache.ContainsKey(intKey))
            //    _cache.Add(intKey, _currentSnapShot);


            var mapInfo = _engine.World.Map.GetMapInfo();
        }

        private void MakeIteration()
        {
            //            if (_currentExchangeSnapshot != null)
            //                {
            //                    var agents = new List<IGuestAgent>();
            //                    while (_currentExchangeSnapshot.Count > 0)
            //                    {
            //                        var shstAgents = (_currentExchangeSnapshot.Dequeue() as PulseExchangeSnapshot).Agents;
            //                        agents.AddRange(shstAgents.OfType<IGuestAgent>());
            //                    }
            //                    var allguests = agents;
            //
            //                    _currentExchangeSnapshot = new Queue<IExchangeSnapshot>();
            //
            //                    (_engine.World as PulseWorld).AddGuestAgents(allguests);
            //                }

            _engine.DoStep();
        }

        private double GetIterationTime()
        {
            if (_fps == 0) return 0;
            return 1 * 1000 / _fps; //ms
        }
    }
}
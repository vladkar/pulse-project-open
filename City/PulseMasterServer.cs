using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using City.ControlsClient;
using City.ControlsServer;
using City.Snapshot;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using City.UIFrames.Impl;
using Fusion.Engine.Server;
using Newtonsoft.Json;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.Model;
using Pulse.Model.Agents;
using Pulse.Model.Environment;

namespace City
{
    public class PulseMasterServer : GameServer
    {
        private EnumElement _state;
        private IDictionary<int, IPulseServer> _controls;
        private IDictionary<int, ControlInfo> _config;

        public string Map { get; protected set; }
        public Orientation ViewLayout;


        public PulseMasterServer(Game gameEngine) : base(gameEngine)
        {
            _state = PulseRunnerState.NotInitialized;
            _controls = new ConcurrentDictionary<int, IPulseServer>();
            _config = new ConcurrentDictionary<int, ControlInfo>();
        }


        public override void Initialize()
        {
            _state = PulseRunnerState.Initializing;

            

            _state = PulseRunnerState.Initialized;
        }

        public override void LoadContent(string map)
        {
            _state = PulseRunnerState.Loading;
            Map = map;
            
            ConfigureLevel(map);

//            _snapshotUtil = new SnapshotUtil(GetServerInfo());

            foreach (var control in _controls.Values)
                control.Initialize(this);

            //            _controls.AsParallel().ForAll(c => c.Value.LoadLevel(_config[c.Key]));
            foreach(var control in _controls)
                control.Value.LoadLevel(_config[control.Key]);

            _state = PulseRunnerState.Loaded;
        }

//        private SnapshotUtil _snapshotUtil;

        private void ConfigureLevel(string map)
        {
            // panels on the screen:
            // 1 2
            // 3 4

	        string scen = "train";

            switch (map.ToLower())
            {
                case "vk":
                    map = "vkfeststage";
                    goto case "vkfeststage";
                case "vkfeststage":
                    _controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.PulseAgentControl,
                        Scenario = map,
                        Fps = 15,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "not implemented :("} },
                            {2, new ViewInfo {Panel  = 2, View = "not implemented :("} }
                        }
                    };
                    ViewLayout = Orientation.TwoVertical;
                    break;


                case "ht":
                    map = "harsiddhitemple";
                    goto case "harsiddhitemple";
                case "harsiddhitemple":
                    _controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.PulseAgentControl,
                        Scenario = map,
                        Fps = map.ToLower().Contains("harsiddhi") ? 50 : 0,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "harsiddhitemple2d"} },
                            {2, new ViewInfo {Panel  = 2, View = "harsiddhitemple3d"} }
                        },
                        
                        //TODO Panels = new Dictionary<int, ViewInfo> { Panel {Id = 1, Type = GisBlackMap}}
                    };
                    ViewLayout = Orientation.TwoVertical;
                    break;

                case "mt":
                    map = "mahakaltemple";
                    goto case "mahakaltemple";
                case "mahakaltemple":
                    _controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.PulseAgentControl,
                        Scenario = map,
                        Fps = map.ToLower().Contains("mahakal") ? 50 : 0,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "mahakaltemple2d"} },
                            {2, new ViewInfo {Panel  = 2, View = "mahakaltemple3d"} }
                        },
                        
                        //TODO Panels = new Dictionary<int, ViewInfo> { Panel {Id = 1, Type = GisBlackMap}}
                    };
                    ViewLayout = Orientation.TwoHoriz;
                    break;

                case "ins":
                    map = "instagram";
                    goto case "instagram";
                case "instagram":
                    //_controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.InstagramControl,
                        Scenario = map,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "instagramdynview"} },
                        },
                        
                    };
                    ViewLayout = Orientation.Instagram;
                    break;

                case "ss":
                    map = "sochisent";
                    goto case "sochisent";
                case "sochisent":
                    //_controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.SochiSentControl,
                        Scenario = map,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "instagramdynview"} },
                        },

                    };
                    ViewLayout = Orientation.Instagram;
                    break;

                case "tr":
                    map = "simpletrain";
                    goto case "simpletrain";
                case "simpletrain":
                    _controls[1] = new PulseServer(1);
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.TrainControl,
                        Scenario = map,
                        Fps = 100,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "Map"} },
                            {2, new ViewInfo {Panel  = 2, View = "Graph"} },
                            {3, new ViewInfo {Panel  = 3, View = "Train"} }
                        }
                    };
                    ViewLayout = Orientation.Train;
                    break;


                case "vf":
                    map = "vkfest";
                    goto case "vkfest";
                case "vkfest":
                    _config[1] = new FpsControlInfo
                    {
                        Id = 1,
                        Name = GlobalStrings.VkFestControl,
                        Scenario = map,
                        Views = new Dictionary<int, ViewInfo>
                        {
                            {1, new ViewInfo {Panel  = 1, View = "vkfestview"} },
                        },

                    };
                    ViewLayout = Orientation.VkFest;
                    break;


                //                case "krest":
                //                    map = "krestovsky";
                //                    goto case "krestovsky";
                //                case "ht":
                //                    map = "harsiddhitemple";
                //                    goto case "harsiddhitemple";
                //                case "ht3d":
                //                    map = "harsiddhitemple3d";
                //                    goto case "harsiddhitemple3d";
                //
                //                case "simple_test28":
                //                case "krestovsky":
                //                case "novokrest":
                //                case "hospital":
                //                case "harsiddhitemple":
                //                case "harsiddhitemple3d":
                //                    _controls[1] = new PulseServer(1);
                //                    _config[1] = new FpsControlInfo
                //                    {
                //                        Id = 1,
                //                        Name = GlobalStrings.PulseAgentControl,
                //                        Scenario = map,
                //                        Panels = new[] {1},
                //                        Fps = map.ToLower().Contains("harsiddhi") ? 50 : 0
                //                    };
                //                    break;
                //
                //                case "sht":
                //                    var map1 = "harsiddhitemple";
                //                    var map2 = "harsiddhitemple3d";
                //
                //                    _controls[1] = new PulseServer(1);
                //                    _config[1] = new FpsControlInfo
                //                    {
                //                        Id = 1,
                //                        Name = GlobalStrings.PulseAgentControl,
                //                        Scenario = map1,
                //                        Panels = new[] {1},
                //                        Fps = map1.ToLower() == "harsiddhitemple" ? 50 : 0
                //                    };
                //                    _controls[2] = new PulseServer(2);
                //                    _config[2] = new FpsControlInfo
                //                    {
                //                        Id = 2,
                //                        Name = GlobalStrings.PulseAgentControl,
                //                        Scenario = map2,
                //                        Panels = new[] {2},
                //                        Fps = map2.ToLower() == "harsiddhitemple3d" ? 50 : 0
                //                    };
                //                    break;
                //
                //                case "simpletraffic":
                //                case "almazovskiy":
                //                    _controls[1] = new TrafficServer();
                //                    _config[1] = new ControlInfo
                //                    {
                //                        Id = 1,
                //                        Name = GlobalStrings.TrafficAgentControl,
                //                        Scenario = map,
                //                        Panels = new[] {1, 2}
                //                    };
                //                    break;
                //
                //                case "data1":
                //                {
                //                    _config[1] = new ControlInfo {Id = 1, Name = "fms", Scenario = "fms", Panels = new[] {1}};
                //                    _config[2] = new ControlInfo {Id = 2, Name = "emercalls", Scenario = "emercalls", Panels = new[] {2}};
                //                    _config[3] = new ControlInfo {Id = 3, Name = "subway", Scenario = "subway", Panels = new[] {3}};
                //                    _config[4] = new ControlInfo {Id = 4, Name = "survcam", Scenario = "survcam", Panels = new[] {4}};
                //
                //                    break;
                //                }
                //
                //                case "data2":
                //                {
                //                    _config[1] = new ControlInfo {Id = 1, Name = "fms", Scenario = "fms", Panels = new[] {1}};
                //                    _config[2] = new ControlInfo {Id = 2, Name = "sent", Scenario = "sent", Panels = new[] {2}};
                //                    _config[3] = new ControlInfo {Id = 3, Name = "whores", Scenario = "whores", Panels = new[] {3}};
                //                    _config[4] = new ControlInfo {Id = 4, Name = "inst", Scenario = "inst", Panels = new[] {4}};
                //
                //                    break;
                //                }
                //
                //                case "scale":
                //                {
                //                    _config[1] = new ControlInfo {Id = 1, Name = "fms", Scenario = "fms", Panels = new[] {1, 4}};
                //                    _config[2] = new ControlInfo
                //                    {
                //                        Id = 2,
                //                        Name = GlobalStrings.PulseAgentControl,
                //                        Scenario = "krestovsky",
                //                        Panels = new[] {2, 4}
                //                    };
                //                    _config[3] = new ControlInfo {Id = 3, Name = "inst", Scenario = "inst", Panels = new[] {3}};
                //
                //                    break;
                //                }

                default:

                    if (map.ToLower().Contains("train"))
                    {
                        _controls[1] = new PulseServer(1);
                        _config[1] = new FpsControlInfo
                        {
//                                                        Scenario = $"Sirius{Path.DirectorySeparatorChar}{map.ToLower()}",
                            Scenario = map.ToLower(),
                            Name = GlobalStrings.TrainControl,
                            Fps = 150,
                            Id = 1,
                            Views = new Dictionary<int, ViewInfo>
                            {
                                {1, new ViewInfo {Panel = 1, View = "Map"}},
                                {2, new ViewInfo {Panel = 2, View = "Graph"}},
                                {3, new ViewInfo {Panel = 3, View = "Train"}}
                            },
                        };
                        ViewLayout = Orientation.Train;
                        break;
                    }


                    throw new Exception($"Scenario {map} not found");
            }
        }

        public override void UnloadContent()
        {
            _state = PulseRunnerState.Unloading;

            foreach (var control in _controls.Values)
                control.UnloadLevel();

            _state = PulseRunnerState.Finished;
        }

        // TODO write subsnapshot offsets
        // snapshot
        // header: byte: count of controls(from 0 to byte.MaxValue)
        // key: byte: id, int: len
        // value: custom
        public override byte[] Update(GameTime gameTime)
        {
            TargetFrameRate = 10;

            if (_state.Key < PulseRunnerState.Loaded.Key) return new byte[0];

            foreach (var control in _controls.Values.OfType<PulseServer>())
            {
                var ceshts = control.GetExchangeCnaphot();
//                if (ceshts != nulll)
                foreach (var cesht in ceshts.Values)
                {
                    FeedExchangeCnaphot(cesht);
                }
            }

            var completeSnapshot = new Dictionary<int, ISnapshot>();
            foreach (var control in _controls)
            {
                var snapshot = control.Value.Update();
                completeSnapshot[control.Key] = snapshot;
            }

            var pbs = new PulseSnapshotBinarySerializer(_config);
            var currentSnapshot = pbs.SerializeSnapshot(completeSnapshot);

            return currentSnapshot;
        }

		public override void FeedCommand(Guid id, byte[] userCommand, uint commandID, float lag)
		{
            if (_state == PulseRunnerState.Loaded)
            {
                _state = PulseRunnerState.Running;
            }

            else if (_state == PulseRunnerState.Running)
            {
                var pbs = new PulseSnapshotBinarySerializer(_config);
                var shtCmds = pbs.DeserializeCommand(userCommand);

                foreach (var ctrlCmds in shtCmds)
                {
                    if (_controls.ContainsKey(ctrlCmds.Key))
                        foreach (var cmd in ctrlCmds.Value)
                    {
                            _controls[ctrlCmds.Key].FeedCommand("", cmd);
                        }
                }
            }
        }

        public override void FeedNotification(Guid id, string message)
        {
        Log.Message("NOTIFICATION {0}: {1}", id, message);
			NotifyClients("{0} says: {1}", id, message);
        }

        public override string ServerInfo()
        {
            return JsonConvert.SerializeObject(GetServerInfo());
        }

        private ServerInfo GetServerInfo()
        {
            return new ServerInfo {State = _state.Key, Scenario = Map, Controls = _config, ViewLayout = ViewLayout};
        }

        public override void ClientConnected(Guid id, string userInfo)
        {
            //throw new NotImplementedException();
        }

        public override void ClientActivated(Guid guid)
        {
            //throw new NotImplementedException();
        }

        public override void ClientDeactivated(Guid guid)
        {
            //throw new NotImplementedException();
        }

        public override void ClientDisconnected(Guid id)
        {
            //throw new NotImplementedException();
        }

        public override bool ApproveClient(Guid id, string userInfo, out string reason)
        {
            Log.Message("APPROVE: {0} {1}", id, userInfo);
			reason = ".";
			return true;
        }

        public void FeedExchangeCnaphot(IExchangeSnapshot snapshot)
        {
            if (!_controls.ContainsKey(snapshot.DestinationServer))
            {
                //throw new Exception($"Server control {snapshot.DestinationServer} not found for scenatio {Map}");
                Log.Warning($"Server control {snapshot.DestinationServer} as destination for agent teleport not found for scenatio {Map}");
                return;
            }

            var icontrol = _controls[snapshot.DestinationServer] as PulseServer;
            if (icontrol == null)
            {
                //throw new Exception($"Server control {snapshot.DestinationServer} for scenatio {Map} is not interactive: cannot feed server snapshot");
                Log.Warning($"Server control {snapshot.DestinationServer} as destination for agent teleport for scenatio {Map} is not interactive: cannot feed server snapshot");
                return;
            }

            icontrol.FeedExchangeCnaphot(snapshot);
        }
	}

    public class ServerInfo
    {
        public int State { set; get; }
        public string Scenario { set; get; }
        public Orientation ViewLayout { set; get; }
        public IDictionary<int, ControlInfo> Controls { set; get; } 
    }

    public class ControlInfo
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string Scenario { set; get; }
        public IDictionary<int, ViewInfo> Views { set; get; }
//        public int[] Panels { set; get; }
    }

    public class ViewInfo
    {
        public int Panel { set; get; }
        public string View { set; get; }
    }

    public class FpsControlInfo : ControlInfo
    {
        public double Fps { set; get; }
    }

    public static class PulseRunnerState
    {
        public static EnumElement NotInitialized { get; }
        public static EnumElement Initializing { get; }
        public static EnumElement Initialized { get; set; }
        public static EnumElement Loading { get; set; }
        public static EnumElement Loaded { get; set; }
        public static EnumElement Running { get; }
        public static EnumElement Error { get; private set; }
        public static EnumElement Paused { get; }
        public static EnumElement Unloading { get; set; }
        public static EnumElement Finished { get; set; }

        private static EnumElement[] _fields;

        static PulseRunnerState()
        {
            NotInitialized = new EnumElement { Key = 1, Value = "NotInitialized" };
            Initializing = new EnumElement { Key = 2, Value = "Initializing" };
            Initialized = new EnumElement { Key = 3, Value = "Initialized" };
            Loading = new EnumElement { Key = 4, Value = "Loading" };
            Loaded = new EnumElement { Key = 5, Value = "Loaded" };
            Running = new EnumElement { Key = 6, Value = "Running" };
            Paused = new EnumElement { Key = 7, Value = "Paused" };
            Unloading = new EnumElement { Key = 8, Value = "Unloading" };
            Finished = new EnumElement { Key = 9, Value = "Finished" };
            Error = new EnumElement { Key = 10, Value = "Error" };

            _fields = typeof(PulseRunnerState).GetFields(BindingFlags.Public).Select(f => f.GetValue(null) as EnumElement).Where(e => e != null).ToArray();
        }

        public static EnumElement GetState(int id)
        {
            return _fields.First(f => f.Key == id);
        }
    }
}

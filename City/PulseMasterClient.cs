using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using City.ControlsClient;
using City.Snapshot;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Fusion.Core.Configuration;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Client;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Newtonsoft.Json;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Model;
using Pulse.Model.Environment;
using Fusion;
using Pulse.Common.Utils;
using SharpDX;

namespace City
{
    public class PulseMasterClient : GameClient
    {
        public CityConfig Config { get; set; } = new CityConfig();

        public IDictionary<int, IPulseControl> Clients { set { _controls = value; } }
        private IDictionary<int, IPulseControl> _controls;
        //private RenderLayer _masterLayer;
        private ServerInfo _serverConfig;

        //private EnumElement _state;


        public PulseMasterClient(Game gameEngine) : base(gameEngine)
        {
        }


        public override void Initialize()
        {
            //_state = PulseRunnerState.Initializing;

            _controls = new ConcurrentDictionary<int, IPulseControl>();
            Config.DataDir = GetInpuDatatDir();

            //_state = PulseRunnerState.Initialized;
        }
        
        public override GameLoader LoadContent(string serverInfo)
        {
            //_state = PulseRunnerState.Loading;

            if (string.IsNullOrEmpty(serverInfo)) return new CityGameLoader();

            _serverConfig = JsonConvert.DeserializeObject<ServerInfo>(serverInfo);

            (Game.GameInterface as CustomGameInterface).mainFrame.ApplyClient(this);
            (Game.GameInterface as CustomGameInterface).mainFrame.SetServerInfo(_serverConfig);

            //_state = PulseRunnerState.Loaded;

            if (_serverConfig.Scenario.Contains("train") | _serverConfig.Scenario.Contains("tr"))
                Game.Invoker.Push("ui.UILayout  train");
            else
                Game.Invoker.Push("ui.UILayout  ht");
            //UILayout = train

            return new CityGameLoader();
        }

        public class CityGameLoader : GameLoader
        {
            public override void Update(GameTime gameTime)
            {
            }

            public override bool IsCompleted { get; } = true;
        }

        public override void FinalizeLoad(GameLoader loader)
        {
        }

        public override void UnloadContent()
        {
            foreach (var control in _controls.Values)
                control.Unload();
        }


		public override byte[] Update(GameTime gameTime, uint sentCommandID)
		{
            //NotifyServer();

            var commands = new Dictionary<int, ICommandSnapshot[]>();
		    foreach (var control in _controls)
		    {
		        if (control.Value.ControlsState == PulseRunnerState.Loaded)
		        {

		            var shtCmds = control.Value.Update(gameTime);
		            commands[control.Key] = shtCmds;
		        }
		    }

            var pbs = new PulseSnapshotBinarySerializer(_serverConfig.Controls);
            var bytecommands = pbs.SerializeCommand(commands);
            return bytecommands;
        }


		public override void FeedSnapshot(GameTime serverTime, byte[] snapshot, uint ackCommandID)
		{
            var pbs = new PulseSnapshotBinarySerializer(_serverConfig.Controls);
            var currentSnapshot = pbs.DeserializeSnapshot(snapshot);

            (Game.GameInterface as CustomGameInterface).mainFrame.FeedSnapshot(currentSnapshot);
        }
        
        public override void FeedNotification(string message)
        {
            Log.Message("NOTIFICATION : {0}", message);
        }

        public override string UserInfo()
        {
            //throw new NotImplementedException();
            return "test";
        }


        private string GetInpuDatatDir()
        {
            var assemblFile = Assembly.GetExecutingAssembly().Location;
            var assenblDir = Path.GetDirectoryName(assemblFile);
            var solutionDir = Directory.GetParent(assenblDir).Parent.Parent.FullName;

            #region fusion content

            var dataFContentDir = $@"{solutionDir}\Content\Data\";

            if (Directory.Exists(dataFContentDir))
                return dataFContentDir;

            #endregion

            #region  Deployed Assembly

            var dataAssemblyDirLocation = assenblDir + Path.DirectorySeparatorChar + "InputData" + Path.DirectorySeparatorChar;
            var isDataInAssemblyDir = Directory.Exists(dataAssemblyDirLocation);

            if (isDataInAssemblyDir)
                return dataAssemblyDirLocation;

            #endregion

            throw new Exception("Input Data directory not found");
        }
    }


    public class CityConfig
    {
        public string DataDir { get; set; }

        public CityConfig()
        {
            DataDir = "UNDEFINED";
        }
    }
}

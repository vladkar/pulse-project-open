using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Pulse.MultiagentEngine.Containers;
using Pulse.MultiagentEngine.Events;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Generation;
using Pulse.MultiagentEngine.Scenario;
using Pulse.MultiagentEngine.Settings;
using Pulse.MultiagentEngine.Stats;

namespace Pulse.MultiagentEngine.Engine
{
    public enum SimulationRunMode
    {
        StepByStep,
        Continous
    }

    public class SimulationCounters
    {
        public DateTime LastSecond = DateTime.Now;
        public int StepsInSecondCounter = 0;

        private ulong _iterationNumber = 0;
        public ulong IterationNumber
        {
            get { return _iterationNumber; }
            set { _iterationNumber = value; }
        }

        private ulong _totalAgentsTerminated = 0;
        public ulong TotalAgentsTerminated
        {
            get { return _totalAgentsTerminated; }
            set { _totalAgentsTerminated = value; }
        }

        private ulong _totalAgentsGenerated = 0;
        public ulong TotalAgentsGenerated
        {
            get { return _totalAgentsGenerated; }
            set { _totalAgentsGenerated = value; }
        }
    }

    /// <summary>
    /// General class for simulation.
    /// Domain-independant.
    /// </summary>
    public class SimulationEngine : ISimulationControl
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Public API could be translated to network API.
        /// </summary>
        #region External API

        public SimulationState State { get; protected set; }
        public string StateComment { get; protected set; }

        public SimulationRunMode RunMode { get; set; }

        #region Player
        public CommandResult Command(string commandName, object arg)
        {
            try
            {
                switch (commandName)
                {
                    case "pause":
                        Pause();
                        break;
                    case "stop":
                        Stop();
                        break;
                    case "start":
                        Start();
                        break;
                    case "resume":
                        Resume();
                        break;
                    
                    default:
                        if (CommandExecutor != null)
                            CommandExecutor.Command(commandName, arg);
                        else
                            Log.Error("Unknown command '{0}'. Ignored.", commandName);
                        break;
                }
            }
            catch (Exception ex)
            {
                return new CommandResult(CommandResultState.Error, ex.StackTrace);
            }

            return new CommandResult(CommandResultState.Ok);
        }

        public void Pause()
        {
            switch (State)
            {
                case SimulationState.Init:
                    throw new InvalidOperationException("Can not pause not active simulation");
                    break;
                case SimulationState.Active:
                    State = SimulationState.Paused;
                    break;
                case SimulationState.Paused:
                case SimulationState.Stopped:
                case SimulationState.Error:
                case SimulationState.Finished:
                    // ignore
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Resume()
        {
            switch (State)
            {
                case SimulationState.Init:
                    throw new InvalidOperationException("Can not resume not active simulation");
                    break;
                case SimulationState.Active:
                    // ignore
                    break;
                case SimulationState.Paused:
                    State = SimulationState.Active;
                    break;
                case SimulationState.Stopped:
                case SimulationState.Error:
                case SimulationState.Finished:
                    throw new InvalidOperationException("Can not resume not active simulation");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// To finish externally
        /// </summary>
        /// <param name="comment"></param>
        public void Finish(string comment)
        {
            switch (State)
            {
                case SimulationState.Init:
                    throw new InvalidOperationException("Can not stop not active simulation");
                    break;
                case SimulationState.Active:
                case SimulationState.Paused:
                    State = SimulationState.Finished;
                    StateComment = comment;
                    break;
                case SimulationState.Stopped:
                case SimulationState.Error:
                case SimulationState.Finished:
                    // ignore
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public void Stop()
        {
            switch (State)
            {
                case SimulationState.Init:
                    throw new InvalidOperationException("Can not stop not active simulation");
                    break;
                case SimulationState.Active:
                case SimulationState.Paused:
                    State = SimulationState.Stopped;
                    break;
                case SimulationState.Stopped:
                case SimulationState.Error:
                case SimulationState.Finished:
                    // ignore
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Restart()
        {

        }

        public void Start()
        {
            switch (State)
            {
                case SimulationState.Init:
                    State = SimulationState.Active;


                    if (RunMode == SimulationRunMode.Continous)
                    {
                        try
                        {
                            Initialize();
                            RunSimulationCycle();
                            Finalization();
                            State = SimulationState.Finished;
                        }
                        catch (Exception ex)
                        {
                            State = SimulationState.Error;
                        }

                    }
                    else
                    {
                        Initialize();

                        // wait for step commands
                    }


                    break;
                case SimulationState.Active:
                    // ignore
                    break;
                case SimulationState.Paused:
                case SimulationState.Stopped:
                case SimulationState.Error:
                case SimulationState.Finished:
                    Restart();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DoStep()
        {
            if (State == SimulationState.Active)
            {
                Step();
            }
        }

        #endregion

        #endregion
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Private Counters
        /// </summary>
        #region Counters
        public SimulationCounters Counters = new SimulationCounters();
        #endregion
        //----------------------------------------------------------------------------------------------------------------------------------
        private bool _isAgentSourceExhausted = false;
        /// <summary>
        /// Used to determine stop condition for simulation.
        /// Used to optimise step cycle by not calling generation model.
        /// </summary>
        public bool IsAgentSourceExhausted
        {
            get { return _isAgentSourceExhausted; }
            set { _isAgentSourceExhausted = value; }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        #region Setters
        public SimWorld World { get; protected set; }
        public SimulationProperties Properties { get; }
        //public AgentsAggregatedStatistics Stats { get; private set; }
        public IStatisticsRegistry StatRegistry { get; }
        public IAgentGenerationModel GenerationModel { get; set; }
        public SimulationInfo Info { get; set; }
        public ICommandExecutor CommandExecutor { get; set; }
        public GlobalScenario Scenario { get; set; }
        public EventEngine EventEngine { get; set; }
        public IDictionary<string, string> Stats { get; }

        #endregion
        //============================================================================================================================================

        public SimulationEngine(SimWorld world, SimulationProperties properties, IAgentGenerationModel generationModel, SimulationRunMode runMode)
        {
            // params
            World = world;
            World.Engine = this;
            Properties = properties;
            GenerationModel = generationModel;
            RunMode = runMode;

            // defaults
            State = SimulationState.Init;
            //Stats = new AgentsAggregatedStatistics();
            Info = new SimulationInfo();
            StatRegistry = new StatisticsRegistry(this);
            Scenario = new GlobalScenario();
            EventEngine = new EventEngine();
            Stats = new Dictionary<string, string>();

            Log.Info("UseNormalStopCondition was set to " + properties.UseNormalStopCondition);
            Log.Info("TimeStep was set to " + properties.TimeProperties.TimeStep);
            Log.Info("SimulationStartsAt was set to " + properties.TimeProperties.SimulationStartsAt);
            Log.Info("SimulationEndsAt was set to " + properties.TimeProperties.SimulationEndsAt);

            Log.Info("Simulation engine initialized");
        }

        public Func<bool>[] StopConditions { get; set; }

        protected virtual void Step()
        {
            Info.Time = World.Time;
            Info.StepNumber = Counters.IterationNumber;

            //-----------------------------------------------------------
            // Check stop condition
            //-----------------------------------------------------------
            //if (StopConditions.Aggregate(false, (res, cond) => res & cond()))
            //{
            //    State = SimulationState.Finished;
            //    StateComment = "Simulation successfully finished";
            //}

            if (Counters.IterationNumber == 2)
            {
                // TODO: change to explicit start condition sent by syncro messages
                // all connected nodes works
                Info.SecondIteratiobStartedAt = DateTime.Now;
            }

            DateTime beforeStep = DateTime.Now;

            //-----------------------------------------------------------
            // generate new agents
            //-----------------------------------------------------------
            //if (!IsAgentSourceExhausted)
            //{
                if (GenerationModel.HaveMoreAgents(World.Time))
                {
                    var newAgents = GenerationModel.GenerateAgentsInTime(World.Time);
                    World.AddNewAgents(newAgents);
                    Counters.TotalAgentsGenerated += (ulong)newAgents.Length;
                    Info.AgentsGenerated = (uint) newAgents.Length;

//                    if (newAgents.Length> 0)
//                        Log.Trace("{0} agents generated: [{1}]", newAgents.Length, String.Join(",", newAgents.Select(@base => @base.Id)));

                }
//                else
//                {
//                    IsAgentSourceExhausted = true;
//                }
           // }

            //-----------------------------------------------------------
            // update all agents - apply agents steering and behavior));
            //-----------------------------------------------------------

            var isParallel = true;

#if (!DEBUG)
            isParallel = true;
#endif

            if (isParallel)
//                World.Agents.AsParallel().ForAll((base1 => base1.StepAct(Properties.TimeProperties.TimeStep, World.Time)));
                Parallel.ForEach(World.Agents, (a) => a.StepAct(Properties.TimeProperties.TimeStep, World.Time));
            else
            {
                foreach (var agent in World.Agents)
                {
                    agent.StepAct(Properties.TimeProperties.TimeStep, World.Time);
                }
            }

            //-----------------------------------------------------------
            // move the objects on the map
            //-----------------------------------------------------------
            World.Map.Update(Properties.TimeProperties.TimeStep, World.Time);

            //-----------------------------------------------------------
            // update all agents stats
            //StatRegistry.Update();

            //-----------------------------------------------------------
            // Apply scenarios
            Scenario.Apply(this);
            
            //-----------------------------------------------------------
            // Infos
            Info.AgentsCount = (ulong)World.Agents.Count;
            Counters.IterationNumber++;

            //-----------------------------------------------------------
            // visualize

            //-----------------------------------------------------------
            // update time
            //-----------------------------------------------------------
            
            World.Time = Math.Round(World.Time + Properties.TimeProperties.TimeStep, 5);

            //-----------------------------------------------------------
            // reset calc stats for step
            //-----------------------------------------------------------
            //Stats.OnStepFinalize();

            //-----------------------------------------------------------
            // clean dead agents
            //-----------------------------------------------------------
            // TODO: optimize
            var deadies = World.Agents.Where(agentBase => !agentBase.IsAlive).ToList();
            Counters.TotalAgentsTerminated += (ulong)deadies.Count;

//            if(deadies.Count > 0)
//                Log.Trace("{0} agents terminated: [{1}]", deadies.Count, String.Join(",", deadies.Select(@base => @base.Id)));

            foreach (var deady in deadies)
            {
                // Important: if you change this, change stop condition by checking only alive agents
                World.Agents.Remove(deady);
                World.DeadAgents.Add(deady);
            }


            //-----------------------------------------------------------
            // Check stop condition
            //-----------------------------------------------------------
            // only alive agents
            //if(World.Agents.Count(@base => @base.IsAlive) == 0)
//            if (Properties.UseNormalStopCondition)
//            {
//                if (World.Agents.Count == 0)
//                {
//                    if (IsAgentSourceExhausted)
//                    {
//                        State = SimulationState.Finished;
//                        StateComment =
//                            "Simulation successfully finished according to normal stop condition: abscence of alive agents and abscence of agents in generation model";
//                    }
//                }
//            }

            DateTime afterStep = DateTime.Now;

            Counters.StepsInSecondCounter++;

            if (afterStep - Counters.LastSecond >= new TimeSpan(0, 0, 0, 1))
            {
                Info.StepsPerSecond = (float)((double)Counters.StepsInSecondCounter / (afterStep - Counters.LastSecond).TotalSeconds);
                Counters.StepsInSecondCounter = 0;
                Counters.LastSecond = DateTime.Now;
            }

            Info.StepExecutionTime = (float)(afterStep - beforeStep).TotalMilliseconds;

        }


        protected void Initialize()
        {
            // Initialization
            Info.WorldName = World.Name;

            Info.StepNumber = 0;
            Info.StartedAt = DateTime.Now;

            Info.ConfigurationTimeStep = (float)Properties.TimeProperties.TimeStep;
            Info.ConfigurationToSecondsMultiplier = (float)Properties.TimeProperties.ToSecondsMultiplier;
            Info.ConfigurationSimulationEndsAt = (float)Properties.TimeProperties.SimulationEndsAt;

            // Set start time
            World.Time = Properties.TimeProperties.SimulationStartsAt;

            Info.SimTimeStart = (float)World.Time;

            Log.Info("Simulation started at {0}", DateTime.Now);

            // ignore agents generated before SimulationStartsAt
            GenerationModel.GenerateAgentsInTime(Properties.TimeProperties.SimulationStartsAt - 1.0);

            Counters.LastSecond = DateTime.Now;
        }

        public void Finalization()
        {
            // finalize
            Info.State = State.ToString();
            Info.FinishedAt = DateTime.Now;
            Info.SimTimeEnd = (float)World.Time;
            Info.SimulationToRealTimeRate = (float)((World.Time - Properties.TimeProperties.SimulationStartsAt) / (Info.FinishedAt - Info.SecondIteratiobStartedAt).TotalSeconds);

            StatRegistry.Finalization();

            Log.Info("Simulation finished at {0}", DateTime.Now);
        }

        public TimestampedData<IAgentsData> GetAgentsData()
        {
            var ret = new TimestampedData<IAgentsData>(World.Time, World.Map.GetAgentsData());
            return ret;
        }

        public TimestampedData<IMapData> GetMapData()
        {
            var ret = new TimestampedData<IMapData>(World.Time, World.Map.GetMapData());
            return ret;
        }

        public TimestampedData<IMapInfo> GetMapInfo()
        {
            var ret = new TimestampedData<IMapInfo>(World.Time, World.Map.GetMapInfo());
            return ret;
        }

        protected void RunSimulationCycle()
        {
            while (State != SimulationState.Finished && State != SimulationState.Error && State != SimulationState.Stopped
                && World.Time <= Properties.TimeProperties.SimulationEndsAt)
            {
                Info.State = State.ToString();
                Log.Debug("=============== ITERATION #{0} ================", Counters.IterationNumber);
                Step();
            }

        }


        public TimestampedData<IAgentsData> GetDeadAgentsData()
        {
            var ret = new TimestampedData<IAgentsData>(World.Time, World.Map.GetDeadAgentsData());
            return ret;
        }
    }

}

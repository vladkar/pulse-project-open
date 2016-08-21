using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Integr;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace Pulse.Common.Model.Environment.World
{
    public class PulseWorld : AbstractGeoWorld
    {
        public PulseScenarioConfig ScenarioConfig { set; get; }
        public int Id { get; set; }

        public PulseWorld(PulseScenarioConfig scenarioConfig) : base(scenarioConfig.GeoWorldInfo.GeoTime, scenarioConfig.Name.Value)
        {
            ScenarioConfig = scenarioConfig;
        }


        public override GeoWorldGeneralInfo GetGeoWorldInfo()
        {
            return new GeoWorldGeneralInfo { GeoTime = GeoTime, MetersPerMapUnit = ScenarioConfig.MapConfig.MetersPerMapUnit, ToSecondsMultiplier = ScenarioConfig.GeoWorldInfo.ToSecondsMultiplier };
        }


        private List<IExternalPortal> _portals;

        public void AddGuestAgents(IList<IGuestAgent> guests)
        {
            EnumeratePortals();

            foreach (var guest in guests)
            {
                try
                {
                    var portal = _portals.Where(p => p.Types.Any(t => t.Name.GetHashCode() == guest.Portal)).RandomChoise();
                    (portal.AgentGenerator as WormHoleAgentsGenerator).DropGuest(guest);
                }
                catch (Exception ex) when (ex is RandomUtilException || ex is NullReferenceException)
                {
                    Log.Warn($"Guest agent from external world can not be added, because required portal with hash {guest.Portal} from world {guest.HomeWorld} to {guest.DestWorld} is missing");
                }
            }
        }


        //TODO USE IT ON UPDATE ONLY: THREAD SAFE CHECKING DID NOT PERFORM
        public IList<IGuestAgent> GetTravelersAgents()
        {
            EnumeratePortals();

            // 0 destworld == nowhere
            var travelers =
                _portals.Select(p => p.AgentGenerator)
                    .OfType<WormHoleAgentsGenerator>()
                    .SelectMany(g => g.GetTravelers()).Where(t => t != null && t.DestWorld != 0).ToList();

            return travelers;
        }

        private void EnumeratePortals()
        {
            if (_portals == null)
                _portals = (Map as IPulseMap).Scenery.Levels.Values.SelectMany(l => l.ExternalPortals).Where(p => p.AgentGenerator is WormHoleAgentsGenerator).ToList();
        }
    }
}

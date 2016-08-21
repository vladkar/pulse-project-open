using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model.Legend;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;

namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class InfectionStateManager : AbstractDataBroker, ILegendable
    {
        private InfectionDataReader _ifr;

        public InfectionStateManager(InfectionDataReader ifr)
        {
            _ifr = ifr;
            Name = "Infection state manager";
        }

        public InfectionInfo GetInfectionInfo()
        {
            return _ifr.InfectionInfo;
        }

        public BaseInfectionStage GetDefaultState()
        {
            return RandomUtil.RandomInt(0, 100) == 50 ? GetImmunityStage() : GetHealthyStage();
        }

        public BaseInfectionStage GetHealthyStage()
        {
            return new BaseInfectionStage
            {
                InfectionState = BaseInfectionStage.InfectionStates.S
            };
        }

        public BaseInfectionStage GetImmunityStage()
        {
            if (_ifr.InfectionStates.ContainsKey(BaseInfectionStage.InfectionStates.IM))
            
                return new BaseInfectionStage
                {
                    InfectionState = BaseInfectionStage.InfectionStates.IM
                };
            
        else
                return new BaseInfectionStage{InfectionState = BaseInfectionStage.InfectionStates.S};
        }

        public BaseInfectionStage GetInfectedStage()
        {
            if (_ifr.InfectionStates.ContainsKey(BaseInfectionStage.InfectionStates.I2))

                return new BaseInfectionStage
                {
                    InfectionState = BaseInfectionStage.InfectionStates.I2
                };

            else
                return new BaseInfectionStage { InfectionState = BaseInfectionStage.InfectionStates.S };
        }

        public BaseInfectionStage GetInfectedStage(DateTime startTime)
        {
            var newInfectedState = BaseInfectionStage.InfectionStates.E;

            return new BaseInfectionStage
            {
                InfectionState = newInfectedState,
                Min = _ifr.GetConstraintMin(newInfectedState),
                Max = _ifr.GetConstraintMax(newInfectedState),
                StartTime = startTime
            };
        }

        public BaseInfectionStage UpdateStage(BaseInfectionStage currentStage, DateTime now)
        {
            var timeInStage = now - currentStage.StartTime;
            
            if (timeInStage >= currentStage.Min)
            {
                BaseInfectionStage.InfectionStates stage;
                if (timeInStage >= currentStage.Max && currentStage.Max.TotalSeconds > 0)
                    stage = _ifr.GetStateExcludeSafe(currentStage.InfectionState);
                else
                {
                    stage = _ifr.GetState(currentStage.InfectionState);
                }
                
                return new BaseInfectionStage
                {
                    InfectionState = stage,
                    Min = _ifr.GetConstraintMin(stage),
                    Max = _ifr.GetConstraintMax(stage),
                    StartTime = currentStage.InfectionState == stage ? currentStage.StartTime : now
                };
            }

            return currentStage;
        }

        protected override void LoadData()
        {
            if (!_ifr.IsLoaded)
                _ifr.Initialize();
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>>
            {
                {
                    "DiseaseStates",
                    _ifr.InfectionStates.Select(s => new LegendElement
                    {
                        Id = (int) s.Key,
                        NiceName = s.Value,
                        Name = Enum.GetName(typeof (BaseInfectionStage.InfectionStates), s.Key)
                    }).ToList()
                }
            };
        }
    }
}

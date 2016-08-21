using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils.Interval;
using Pulse.Social.Population;


namespace Pulse.Social.Data
{
    public class SocialEconomyClassReader : AbstractFileDataReader
    {
        public IList<SocialEconomyClass> Classes { get; private set; }

        //TODO replace to manager 
        private PulseScenery _sb; //(typed building reader)

        public SocialEconomyClassReader(string direPath, PulseScenery sb)
            : base(direPath)
        {
            _sb = sb;
            DataFile = @"population\classes_economy.json";
            Name = "Social-economy class reader";
        }

        protected override void LoadData()
        {
            if (!_sb.IsLoaded)
                _sb.Initialize();


            var jsnClasses = JArray.Parse(File.ReadAllText(DataFullPath));

            var classes = jsnClasses.Select(c =>
            {
                var cls = new SocialEconomyClass
                {
                    Id = c["id"].Value<int>(),
                    Name = c["name"].Value<string>(),
                    HomeTypes = c["home_buildings"].Select(t => _sb.GetPoiTypeByName(t.ToString())).Where(n => n != null).ToList<PointOfInterestType>(),
                    AbstractClassActivities = c["activities"].Select(a => GetAbstractClassActivity(a)).ToList()
                };
                cls.AbstractClassSchedules = c["schedules"].Select(s => GetAbstractClassShedules(s, cls.AbstractClassActivities)).ToList();

                return cls;
            }).ToList();

            Classes = classes;
        }

        private AbstractPoiActivity GetAbstractClassActivity(JToken activity)
        {
            return new AbstractPoiActivity
            {
                ActivityType = activity["type"].Value<string>(),
                Name = activity["name"].Value<string>(),
                PossiblePoiTypes = activity["buildings"].Select(t => _sb.GetPoiTypeByName(t.ToString())).Where(n => n != null).ToList<PointOfInterestType>()
            };
        }

        private AbstractClassSchedule GetAbstractClassShedules(JToken shedule, IList<AbstractPoiActivity> abstractClassActivities)
        {
            var sh = 
             new AbstractClassSchedule
            {
                Name = shedule["name"].Value<string>(),
                Raiting = Int32.Parse(shedule["rating"].Value<string>()),
                Weekdays = shedule["weekdays"].Select(s =>
                {
                    var activity = new AbstractScheduleActivity
                    {
                        Rate = s["per_day_percent"].Value<int>(),
                        PoiActivity = abstractClassActivities.First(p => p.Name == s["activity"].Value<string>()),
                    };
                    // start time or duration (after all)
                    if (s["time"] == null)
                    {
                        var start = s["time_start"].Value<string>().Split('-');
                        var end = s["time_end"].Value<string>().Split('-');

                        activity.TimeStart = start.Length == 1
                            ? new TimeSpanInterval(start[0])
                            : new TimeSpanInterval(start[0], start[1]);

                        activity.TimeEnd = end.Length == 1
                            ? new TimeSpanInterval(end[0])
                            : new TimeSpanInterval(end[0], end[1]);
                    } else
                    {
                        var duration = s["time"].Value<string>().Split('-');
                        activity.TimeDuration = duration.Length == 1
                            ? new MinuteInterval(Int32.Parse(duration[0]))
                            : new MinuteInterval(Int32.Parse(duration[0]), Int32.Parse(duration[1]));
                    }
                    return activity;
                }).ToList()
            };

            return sh;
        }
    }
}

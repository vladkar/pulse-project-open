//using System.Collections.Generic;
//using Pulse.Common.Model.Scheduling._1_Abstract;
//
//namespace Pulse.Common.Model.Scheduling
//{
//    public class AbstractPoiActivity : AbstractAgentActivity
//    {
//        public IList<AbstractPointOfInterestType> PossiblePoiTypes { set; get; }
//    }
//
//    public class AbstractScheduleActivity
//    {
//        public AbstractPoiActivity PoiActivity { set; get; }
//        public string Name { set; get; }
//        public int DailyRaiting { set; get; }
//
//        public TimeSpanInterval TimeStart { set; get; }
//        public TimeSpanInterval TimeEnd { set; get; }
//        public MinuteInterval TimeDuration { set; get; }
//        public string Zone { set; get; }
//        public int Level { set; get; }
//    }
//}
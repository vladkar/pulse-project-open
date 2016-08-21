using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace City.ControlsClient.DomainClient.Train
{
	public class TrainSchedule
	{
		public string Title { get; set; }
		public StopPlace[] StopsPlacesList { get; set; }
	}

	public class StopPlace
	{
		public string Title { get; set; }
		public string CityName { get; set; }
		public string CityNiceName { get; set; }
        public DateTime ArrivalTime { get; set; }
		public DateTime DepartureTime { get; set; }
		public double Latitude { get; set; }
		public double Longtitude { get; set; }
	}


}

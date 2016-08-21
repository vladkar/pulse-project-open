using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace City.ControlsClient.DomainClient.Train
{
	public class TrainRoute
	{
		Game game;

		public TrainSchedule Schedule;
		public DVector2[]	RouteGeoPoints;
		public double[]		Distances;
		public int[]		TrainStationsIndeces;

		public LinesGisLayer	LineLayer;
		public PolyGisLayer		PolyRouteLayer;
		public Texture2D		Texture;

		public double MaxCameraDistance = 3500;
		public double FullRouteViewYaw		;
		public double FullRouteViewPitch	;


		public TrainRoute(Game game, TrainSchedule schedule, DVector2[] routePoints)
		{
			this.game = game;

			Texture			= game.Content.Load<Texture2D>("Train/Railroads");
			Schedule		= schedule;
			RouteGeoPoints	= routePoints;

			// Calculate distances
			Distances = new double[RouteGeoPoints.Length];
			Distances[0] = 0;

			for(int i = 1; i < RouteGeoPoints.Length; i++) {
				Distances[i] = GeoHelper.DistanceBetweenTwoPoints(DMathUtil.DegreesToRadians(RouteGeoPoints[i]), DMathUtil.DegreesToRadians(RouteGeoPoints[i-1])) + Distances[i-1];
			}

			// Find city indeces
			 TrainStationsIndeces = new int[Schedule.StopsPlacesList.Length];
			for(int cityInd = 0; cityInd < Schedule.StopsPlacesList.Length; cityInd++) {
				var city = Schedule.StopsPlacesList[cityInd];
                var cityPoint = new DVector2(city.Longtitude, city.Latitude); 

				double minDist = (cityPoint - RouteGeoPoints[0]).Length();
				
				for(int i = 0; i < RouteGeoPoints.Length; i++) {
					var dist = (cityPoint - RouteGeoPoints[i]).Length();
                    if (dist < minDist) {
						minDist = dist;
						TrainStationsIndeces[cityInd] = i;
                    }
				}
			}


			// Setup thin lines
			LineLayer = new LinesGisLayer(game, (RouteGeoPoints.Length - 1) * 2);
			LineLayer.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE);
			LineLayer.ZOrder = 2000;

			int ind = 0;
			for(int i = 0; i < RouteGeoPoints.Length-1; i++) {
				LineLayer.PointsCpu[ind++] = new Gis.GeoPoint {
					Lon = DMathUtil.DegreesToRadians(RouteGeoPoints[i].X),
					Lat = DMathUtil.DegreesToRadians(RouteGeoPoints[i].Y),
					Color = Color.Yellow,
					Tex0 = new Vector4(),
					Tex1 = new Vector4()
				};
				LineLayer.PointsCpu[ind++] = new Gis.GeoPoint {
					Lon = DMathUtil.DegreesToRadians(RouteGeoPoints[i+1].X),
					Lat = DMathUtil.DegreesToRadians(RouteGeoPoints[i+1].Y),
					Color = Color.Yellow,
					Tex0 = new Vector4(),
					Tex1 = new Vector4()
				};
			}
			LineLayer.UpdatePointsBuffer();


			PolyRouteLayer			= PolyGisLayer.CreateRoadFromLine(RouteGeoPoints, Distances, 0.1);
			PolyRouteLayer.Texture	= Texture;
			PolyRouteLayer.ZOrder	= 2000;
        }


		public double GetDistanceByTime(DateTime time)
		{
			if (time <= Schedule.StopsPlacesList.First().DepartureTime)		return Distances[TrainStationsIndeces[0]];
			if (time >= Schedule.StopsPlacesList.Last().ArrivalTime)		return Distances[TrainStationsIndeces.Last()];

			
			for(int ind = 1; ind < Schedule.StopsPlacesList.Length; ind++) {
				var dest	= Schedule.StopsPlacesList[ind];
				var depart	= Schedule.StopsPlacesList[ind-1];

				// Check if train on station
				if (time >= dest.ArrivalTime && time <= dest.DepartureTime)
					return Distances[TrainStationsIndeces[ind]];

				// If we get here that means that train on its path
				if(time > depart.DepartureTime && time < dest.ArrivalTime) {
					var destDist	= Distances[TrainStationsIndeces[ind]];
					var departDist	= Distances[TrainStationsIndeces[ind-1]];

					var distanceBetweenCities = destDist - departDist;

					var timeInTravel	= time - depart.DepartureTime;
					var travelTime		= dest.ArrivalTime - depart.DepartureTime;

					double travelAmount = (double)timeInTravel.Ticks / (double)travelTime.Ticks;

					return departDist + distanceBetweenCities * travelAmount;
				}
			}

			// If we get here that means that something realy wrong happen
			return 0;
		}


		public DVector2 GetPointByDistance(double distance)
		{
			if (distance <= 0)					return RouteGeoPoints[0];
			if (distance >= Distances.Last())	return RouteGeoPoints.Last();

			for(int i = 0; i < Distances.Length-1; i++) {
				var cur		= Distances[i];
				var next	= Distances[i + 1];

				if (Math.Abs(distance - cur) < 0.00001) return RouteGeoPoints[i];

				if(distance > cur && distance < next) {
					var distAmount = (distance - cur)/(next - cur);

					var cartCur		= GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(RouteGeoPoints[i]));
					var cartNext	= GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(RouteGeoPoints[i+1]));

					var cartPoint = DVector3.Lerp(cartCur, cartNext, distAmount);

					var sphPointRad = GeoHelper.CartesianToSpherical(cartPoint);

					return new DVector2(DMathUtil.RadiansToDegrees(sphPointRad.X), DMathUtil.RadiansToDegrees(sphPointRad.Y));
				}
			}

			return RouteGeoPoints[0];
		}


		public DVector2 GetPointByTime(DateTime time)
		{
			var dist = GetDistanceByTime(time);
			return GetPointByDistance(dist);
		}


		public StopPlace IsTrainOnStation(DateTime time)
		{
			if (time < Schedule.StopsPlacesList.First().ArrivalTime)	return Schedule.StopsPlacesList.First();
			if (time > Schedule.StopsPlacesList.Last().ArrivalTime)		return Schedule.StopsPlacesList.Last();

			return Schedule.StopsPlacesList.FirstOrDefault(stop => time >= stop.ArrivalTime && time <= stop.DepartureTime);
		}


		public List<StopPlace> GetNearestCities(DateTime time)
		{
			var ret = new List<StopPlace>();
			var sl = Schedule.StopsPlacesList;

			if (sl.Length < 2) return ret;

			if (time < sl.First().ArrivalTime) {
				ret.Add(sl[0]);
				ret.Add(sl[1]);
				return ret;
			}
			if (time > sl.Last().ArrivalTime) {
				ret.Add(sl[sl.Length-1]);
				ret.Add(sl[sl.Length-2]);
				return ret;
			}


			for(int ind = 1; ind < Schedule.StopsPlacesList.Length; ind++) {
				var dest	= Schedule.StopsPlacesList[ind];
				var depart	= Schedule.StopsPlacesList[ind-1];

				// Check if train on station
				if (time >= dest.ArrivalTime && time <= dest.DepartureTime) {
					ret.Add(sl[ind - 1]);
					ret.Add(sl[ind]);
					ret.Add(sl[ind + 1]);

					break;
				}

				// If we get here that means that train on its path
				if(time > depart.DepartureTime && time < dest.ArrivalTime) {
					ret.Add(sl[ind - 1]);
					ret.Add(sl[ind]);
					break;
				}
			}
			
			return ret;
		}


		public StopPlace GetNearestCity(DateTime time)
		{
			if (time < Schedule.StopsPlacesList.First().DepartureTime)	return Schedule.StopsPlacesList.First();
			if (time > Schedule.StopsPlacesList.Last().ArrivalTime)		return Schedule.StopsPlacesList.Last();

			long	minTicks	= (time - Schedule.StopsPlacesList.First().DepartureTime).Ticks;
			int		minInd		= 0;

            for (int i = 0; i < Schedule.StopsPlacesList.Length; i++) {
				long depTicks = Math.Abs((time - Schedule.StopsPlacesList[i].DepartureTime).Ticks);
				long arrTicks = Math.Abs((time - Schedule.StopsPlacesList[i].ArrivalTime).Ticks);

	            if (depTicks < minTicks) {
		            minTicks	= depTicks;
                    minInd		= i;
	            }

				if (arrTicks < minTicks) {
		            minTicks	= arrTicks;
                    minInd		= i;
	            }
			}

			return Schedule.StopsPlacesList[minInd];
		}


		public static TrainRoute LoadFromFiles(Game game, string scheduleFileName, string routePointsFileName)
		{
			var scheduleString = File.ReadAllText(scheduleFileName);
			
			var serializer	= new JsonSerializer();
			var reader		= new JsonTextReader(new StringReader(scheduleString));
			var schedule	= (TrainSchedule)serializer.Deserialize(reader, typeof (TrainSchedule));

			var routeStrings = File.ReadAllLines(routePointsFileName);
			var points = new DVector2[routeStrings.Length];

			for (int i = 0; i < points.Length; i++) {
				var vals	= routeStrings[i].Split(' ');
				points[i]	= new DVector2(double.Parse(vals[1], CultureInfo.InvariantCulture), double.Parse(vals[0], CultureInfo.InvariantCulture));
			}

			return new TrainRoute(game, schedule, points);
		}


		public static TrainRoute GenerateRouteForDebug(Game game)
		{
			var currentTime = DateTime.Now;
			var schedule	= new TrainSchedule {
				Title = "Debug Route",
				StopsPlacesList = new StopPlace[] {
					new StopPlace { Title = "Sochi",			ArrivalTime = currentTime,							DepartureTime = currentTime + TimeSpan.FromHours(1),	Longtitude = 39.728295, Latitude = 43.591482 },
					new StopPlace { Title = "Loo",				ArrivalTime = currentTime + TimeSpan.FromHours(3),	DepartureTime = currentTime + TimeSpan.FromHours(4),	Longtitude = 39.587013, Latitude = 43.699166 },
					new StopPlace { Title = "Yakornaya Schel",	ArrivalTime = currentTime + TimeSpan.FromHours(7),	DepartureTime = currentTime + TimeSpan.FromHours(8),	Longtitude = 39.505420, Latitude = 43.761196 },
					new StopPlace { Title = "Lazarevskoe",		ArrivalTime = currentTime + TimeSpan.FromHours(14), DepartureTime = currentTime + TimeSpan.FromHours(15),	Longtitude = 39.320975, Latitude = 43.915049 },
				}
			};

			var routePoints = new DVector2[] {
				new DVector2(39.728917, 43.591366), // Sochi
				new DVector2(39.731455, 43.598311),
				new DVector2(39.722271, 43.614563),
				new DVector2(39.699526, 43.622237),
				new DVector2(39.608417, 43.668368),
				new DVector2(39.587334, 43.699256), // Loo
				new DVector2(39.524560, 43.746235),
				new DVector2(39.516535, 43.753133),
				new DVector2(39.505205, 43.761031), // Yakornaya schel
				new DVector2(39.498231, 43.765922),
				new DVector2(39.480915, 43.776377),
				new DVector2(39.466431, 43.785239),
				new DVector2(39.460745, 43.792496),
				new DVector2(39.449115, 43.803825),
				new DVector2(39.431326, 43.824984),
				new DVector2(39.407701, 43.848456),
				new DVector2(39.382961, 43.868585),
				new DVector2(39.366803, 43.881980),
				new DVector2(39.348049, 43.896037),
				new DVector2(39.335003, 43.905708),
				new DVector2(39.325969, 43.911459),
				new DVector2(39.320680, 43.914809), // Lazarevskoe
			};

			return new TrainRoute(game, schedule, routePoints);
		}
	}
}

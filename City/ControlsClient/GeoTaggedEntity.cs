using System;
using System.Collections.Generic;
using System.Globalization;

namespace City.ControlsClient
{
    public class GeoTaggedEntity
    {
        public string Id { set; get; }
        public double Lat { set; get; }
        public double Lon { set; get; }
    }

    public class InstagramPost : GeoTaggedEntity
    {
        public string Url { set; get; }
        public DateTime TimeStamp { set; get; }
		public string Username { set; get; }
		public string Text { set; get; }
		public string Likes { set; get; }
		public string Place { set; get; }		
		public float Happiness { set; get; }
		public float Neutral { set; get; }
		public float Sadness { set; get; }
		public int SocialNetworkId { set; get; }
		public float Polarity { set; get; }
		public int User_Positive { set; get; }
		public int User_Neutral { set; get; }
		public int User_Negative { set; get; }
	}

	public class SentimentPost : GeoTaggedEntity
    {
        public int SentimentValue { set; get; }
    }

    public class WhorePlace : GeoTaggedEntity
    {
        public int Age { set; get; }
        public int Height { set; get; }
        public int Weight { set; get; }
        public int TittySize { set; get; }
        public int Price1 { set; get; }
        public int Price2 { set; get; }
        public int Price3 { set; get; }
    }

    public class SubwayStation : GeoTaggedEntity
    {
        public string Name { set; get; }
        public List<int> InFlow { set; get; }
        public List<int> OutFlow { set; get; }
     
    }

    public class SurveillanceCamera : GeoTaggedEntity
    {
    }

    public class FakeSurveillanceCamera : SurveillanceCamera
    {
        public string FileName { set; get; }
    }
}
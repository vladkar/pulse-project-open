using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Newtonsoft.Json;

namespace City.ModelServer
{
	public class DiseaseServerRespond
	{
		public int[] infected	{ get; set; }
		public int[] dead		{ get; set; }
		public int[] recovered	{ get; set; }
	}


	public class DiseaseMacroClient
	{
		public static string ServerUrl = "http://localhost:8000/sim?city={0}&disease={1}&infinit={2}&length={3}";

		public static DiseaseServerRespond GetServerRespond(string cityName, string diseaseName, int infinit, int length)
		{
			try {
				var request = (HttpWebRequest) WebRequest.Create(String.Format(ServerUrl, cityName, diseaseName, infinit, length));
				var response = (HttpWebResponse) request.GetResponse();

				var resStream = response.GetResponseStream();

				if (resStream != null) {
					var ser = new JsonSerializer();
					var reader = new JsonTextReader(new StreamReader(resStream));

					var respond = ser.Deserialize<DiseaseServerRespond>(reader);

					resStream.Close();

					return respond;
				}

				return null;
			}
			catch (Exception e) {
				Log.Warning(e.Message);
				return null;
			}
		}
	}
}

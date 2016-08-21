using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Newtonsoft.Json;

namespace City.ControlsClient.DomainClient.VKFest.JsonReader
{
    public class ControllerReader
    {
        private static string BASE_URL = "http://192.168.0.65:9999/";

        private static readonly string FUNCTION = "getdata";
        private static readonly string FROM_TIMESTAMP = "from_timestamp=";

        private static readonly string FUNCTION2 = "getdata2"; 
        private static readonly string START_TIMESTAMP = "start_timestamp=";
        private static readonly string END_TIMESTAMP = "end_timestamp=";
        private static readonly string COUNT = "count=";
        private static readonly string LIMIT_FUNCTION = "?";
        private static readonly string LIMIT_PARARMETER = "&";


        private static HttpWebResponse SendRequest(string url)
        {
            Log.Debug("Req_url:\n"+url);
            var uri = new Uri(url);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            var response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
            return response;
        }

        public static RootObject GetPosts(double fromTimeStamp=0, int countValue = 0)
        {
            var url = BASE_URL + FUNCTION + LIMIT_FUNCTION + FROM_TIMESTAMP + fromTimeStamp + LIMIT_PARARMETER + COUNT +
                      countValue;
            return GetObject<RootObject>(url);
        }

        public static RootObject GetPostsAndSaveFile(string fileName, double fromTimeStamp=0, int countValue = 0)
        {
            var url = BASE_URL + FUNCTION + LIMIT_FUNCTION + FROM_TIMESTAMP + fromTimeStamp + LIMIT_PARARMETER + COUNT +
                      countValue;
            return GetDataAndSaveFile<RootObject>(url, fileName);
        }

        public static T GetDataAndSaveFile<T>(string url, string fileName)
        {
            Stream stream = null;
            StreamReader readStream = null;
            WebResponse response = null;
            MemoryStream streamMemory = null;
            T rootNode = default(T);
            try
            {
                response = SendRequest(url);
                stream = response.GetResponseStream();
                streamMemory = new MemoryStream();
                stream.CopyTo(streamMemory);

                streamMemory.Position = 0;
                var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                streamMemory?.CopyTo(fileStream);

                streamMemory.Position = 0;

                readStream = new StreamReader(streamMemory, Encoding.UTF8);
                var info = readStream.ReadToEnd();
                rootNode = JsonConvert.DeserializeObject<T>(info);

                fileStream.Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            finally
            {
                stream?.Close();
                streamMemory?.Close();;
                readStream?.Close();
                response?.Close();
            }
            return rootNode;
        }

        public static T GetObject<T>(string url)
        {
            Stream stream = null;
            WebResponse response = null;
            StreamReader readStream = null;
            T rootNode = default(T);
            try
            {
                response = SendRequest(url);
                stream = response.GetResponseStream();
                readStream = new StreamReader(stream, Encoding.UTF8);
                var info = readStream.ReadToEnd();
                rootNode = JsonConvert.DeserializeObject<T>(info);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            finally
            {
                stream?.Close();
                readStream?.Close();
                response?.Close();
            }
            return rootNode;
        }
        //http://192.168.0.65:9999/getdata2?start_timestamp=1467987024&end_timestamp=1467987063
        public static RootObject GetPostsFromTo(int startTimeStamp = 0, int endTimeStamp = 0)
        {
            var url = BASE_URL + FUNCTION2 + LIMIT_FUNCTION + START_TIMESTAMP + startTimeStamp + LIMIT_PARARMETER + END_TIMESTAMP + endTimeStamp;
            return GetObject<RootObject>(url);
        }
    }
}

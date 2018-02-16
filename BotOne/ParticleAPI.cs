using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Collections;

using System.Web;
using System.Web.Script.Serialization;


namespace XKParticle
{
    internal class ParticleActionPostConfig
    {
        public string base_url { get; set; }
        public string api_token { get; set; }
        public object device_id { get; set; }
        public string action_name { get; set; }
        public string action_data { get; set; }
    }

    public class ParticleDevice
    {
        public string id { get; set; }
        public string name { get; set; }
        public object last_app { get; set; }
        public string last_ip_address { get; set; }
        public string last_heard { get; set; }
        public int product_id { get; set; }
        public bool connected { get; set; }
        public int platform_id { get; set; }
        public bool cellular { get; set; }
        public string status { get; set; }
        public string pinned_build_target { get; set; }
        public string last_iccid { get; set; }
        public string imei { get; set; }
        public string current_build_target { get; set; }
        public Dictionary<String, String> variables { get; set; }
        public List<string> functions { get; set; }
    }

    public class CoreInfo
    {
        public string last_app { get; set; }
        public string last_heard { get; set; }
        public bool connected { get; set; }
        public string last_handshake_at { get; set; }
        public string deviceID { get; set; }
        public int product_id { get; set; }
    }

    public class ParticleVariableInfo
    {
        public string cmd { get; set; }
        public string name { get; set; }
        public string result { get; set; }
        public CoreInfo coreinfo { get; set; }
    }

    public class ParticleFunctionInfo
    {
        public string id { get; set; }
        public string last_app { get; set; }
        public bool connected { get; set; }
        public int return_value { get; set; }
    }
    class ParticleAPI
    {
        private String _AccessCode;
        public ParticleAPI(String AccessCode)
        {
            _AccessCode = AccessCode;
        }
        private static string WebGet(string url)
        {
            return WebRequest(url, null, "GET");
        }

        private static string WebPost(string url, Dictionary<string, string> requestParameters)
        {
            return WebRequest(url, requestParameters, "POST");
        }

        private static string WebRequest(string url, Dictionary<string, string> requestParameters, String RequestMethod)
        {
            byte[] data = null;
            long datalen = 0;

            //construct post data stream for any paramaeters..
            if (requestParameters != null)
            {
                string postData = "";

                foreach (string key in requestParameters.Keys)
                    postData += key + "=" + requestParameters[key] + "&";

                data = Encoding.ASCII.GetBytes(postData);
                datalen = data.Length;
            }

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            myHttpWebRequest.Method = RequestMethod;
            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.ContentLength = datalen;

//            myHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "");

            if (data != null)
            {
                Stream requestStream = myHttpWebRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }


            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream responseStream = myHttpWebResponse.GetResponseStream();

            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

            string pageContent = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            responseStream.Close();

            myHttpWebResponse.Close();

            return pageContent;
        }

        public List<ParticleDevice> GetDeviceList()
        {
            string sURL = string.Format("https://api.particle.io/v1/devices?access_token={0}", _AccessCode);
            string devicedata = WebGet(sURL);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            List<ParticleDevice> devlist = js.Deserialize<List<ParticleDevice>>(devicedata);

            return devlist;
        }

        public ParticleDevice GetDeviceDetails(String DeviceID)
        {
            string sURL = string.Format("https://api.particle.io/v1/devices/{0}?access_token={1}", DeviceID, _AccessCode);
            string returnjson = WebGet(sURL);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            ParticleDevice returnObj = js.Deserialize<ParticleDevice>(returnjson);

            return returnObj;
        }

        public ParticleVariableInfo GetDeviceVariable(String DeviceID, String VarName)
        {
            string sURL = String.Format("https://api.particle.io/v1/devices/{0}/{1}?access_token={2}", DeviceID, VarName, _AccessCode);
            string returnjson = WebGet(sURL);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            ParticleVariableInfo returnObj = js.Deserialize<ParticleVariableInfo>(returnjson);

            return returnObj;
        }

        public ParticleFunctionInfo CallDeviceFunction(String DeviceID, String FuncName, String FuncParam)
        {
            string url = String.Format("https://api.particle.io/v1/devices/{0}/{1}", DeviceID, FuncName);
            Dictionary<string, string> callparam = new Dictionary<string, string>();
            callparam.Add("params", FuncParam);
            callparam.Add("access_token", _AccessCode);
            string returnjson = WebPost(url, callparam);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            ParticleFunctionInfo returnObj = js.Deserialize<ParticleFunctionInfo>(returnjson);

            return returnObj;
        }

    }
}

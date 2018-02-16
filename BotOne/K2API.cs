using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace K2RestAPI
{
    public class Originator
    {
        public string username { get; set; }
        public string fqn { get; set; }
        public string email { get; set; }
        public string manager { get; set; }
        public string displayName { get; set; }
    }

    public class Actions
    {
        public List<object> nonBatchableActions { get; set; }
        public List<string> batchableActions { get; set; }
        public List<string> systemActions { get; set; }
    }


    public class DataField
    {
        public string name { get; set; }
        public object value { get; set; }
    }

    public class XmlField
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Redirect
    {
        public string fqn { get; set; }
    }

    public class Share
    {
        public string fqn { get; set; }
    }

    public class Sleep
    {
        public int sleepFor { get; set; }
        public string sleepUntil { get; set; }
    }

    public class Unshare
    {
        public string fqn { get; set; }
    }

    public class TaskAction
    {
        /*
        public bool allocate { get; set; }
        public string async { get; set; }
        public Redirect redirect { get; set; }
        public List<Share> share { get; set; }
        public Sleep sleep { get; set; }
        public List<Unshare> unshare { get; set; }
        public bool wake { get; set; }
        */
        public Actions actions { get; set; }
        /*
        public string comment { get; set; }
        public List<DataField> instanceDataFields { get; set; }
        public List<XmlField> instanceXmlFields { get; set; }
        public List<DataField> activityDataFields { get; set; }
        public List<XmlField> activityXmlFields { get; set; }
        */
        public TaskAction()
        {

            // customActions = new List<CustomAction>();

            /*
            allocate = false;
            async = string.Empty;
            wake = false;
            comment = string.Empty;

            redirect = new Redirect();
            sleep = new Sleep();
            share = new List<Share>();
            unshare = new List<Unshare>();

            activityDataFields = new List<K2RestAPI.DataField>();
            activityXmlFields = new List<K2RestAPI.XmlField>();

            instanceDataFields = new List<K2RestAPI.DataField>();
            instanceXmlFields = new List<K2RestAPI.XmlField>();
            */
        }
    }
    public class Task
    {
        public string serialNumber { get; set; }
        public string status { get; set; }
        public bool allocated { get; set; }
        public string taskStartDate { get; set; }
        public int priority { get; set; }
        public string dataURL { get; set; }
        public string viewFlowURL { get; set; }
        public int workflowID { get; set; }
        public string workflowName { get; set; }
        public int workflowInstanceID { get; set; }
        public string workflowInstanceFolio { get; set; }
        public int activityInstanceID { get; set; }
        public int activityInstanceDestinationID { get; set; }
        public string eventName { get; set; }
        public string eventDescription { get; set; }
        public Originator originator { get; set; }
        public int assignReason { get; set; }
        public Actions actions { get; set; }
        public List<object> comments { get; set; }
        public List<DataField> activityDataFields { get; set; }

        public Task()
        {
            activityDataFields = new List<K2RestAPI.DataField>();
        }
    }


    public class WorkflowInstances_Post
    {
        public string folio { get; set; }
        public int expectedDuration { get; set; }
        public int priority { get; set; }
        public List<DataField> dataFields { get; set; }
        public List<XmlField> xmlFields { get; set; }

        public WorkflowInstances_Post()
        {
            dataFields = new List<K2RestAPI.DataField>();
            xmlFields = new List<K2RestAPI.XmlField>();
        }
    }

    public class Tasks_Get
    {
        public int itemCount { get; set; }
        public List<Task> tasks { get; set; }
    }

    class K2API
    {

        private static String _baseURL = "https://trialbb.denallix.com/api/workflow/preview";
        private static String _APIUsrPwd = "administrator:K2pass!";

        private static string WebGet(string url)
        {
            return WebRequest(url, null, "GET");
        }

        private static string WebPost(string url, String postBody)
        {
            return WebRequest(url, postBody, "POST");
        }
        private static string WebPut(string url, String putBody)
        {
            return WebRequest(url, putBody, "PUT");
        }

        private static string WebRequest(string url, String postBody, String RequestMethod)
        {
            byte[] data = (!string.IsNullOrEmpty(postBody)) ? Encoding.ASCII.GetBytes(postBody) : new byte[0];
            long datalen = data.Length;

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            myHttpWebRequest.Method = RequestMethod;
            myHttpWebRequest.ContentType = "application/json";
            myHttpWebRequest.ContentLength = datalen;
            String b64creds = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(_APIUsrPwd));
            myHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, b64creds);

            if (!string.IsNullOrEmpty(postBody))
            {
                StreamWriter rs = new StreamWriter(myHttpWebRequest.GetRequestStream());
                rs.Write(postBody);
                rs.Close();
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


        public List<Task> getTasks()
        {
            // https://hqd-api02-v.k2workflow.com:443/Api/Workflow/preview/

            string sURL = string.Format("{0}/tasks", _baseURL);
            string webdata = WebGet(sURL);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            Tasks_Get tg = js.Deserialize<Tasks_Get>(webdata);

            return tg.tasks;
        }

        //public bool finishTask()

        public string actionTask(string serial, String action) // TODO: Add optional username field 
        {

            string sURL = string.Format(String.Format("{0}/tasks/{1}/actions/{2}", _baseURL, serial, action));

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();

            //construct JSON post data
            string postBody = string.Empty;
            //   if (null != action)
            //       postBody = js.Serialize(action);

            postBody = "{}";

            Console.WriteLine(postBody);

            string returnjson = WebPost(sURL, postBody);

            return returnjson;
        }

        public string startWorkflow(int workflowId, WorkflowInstances_Post wfi) // TODO: Add optional username field 
        {

            string sURL = string.Format(String.Format("{0}/workflows/{1}/instances", _baseURL, workflowId));

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();

            //construct JSON post data
            string postBody = string.Empty;
            if (null != wfi)
                postBody = js.Serialize(wfi);

            string returnjson = WebPost(sURL, postBody);

            return returnjson;
        }


    }
}

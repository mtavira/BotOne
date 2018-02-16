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

    public class CustomAction
    {
        public bool batchable { get; set; }
        public string metaData { get; set; }
        public string name { get; set; }
    }

    public class DataField
    {
        public string name { get; set; }
        public string category { get; set; }
        public string dataType { get; set; }
        public object defaultValue { get; set; }
        public bool hidden { get; set; }
    }

    public class XMLField
    {
        public string value { get; set; }
        public string category { get; set; }
        public string name { get; set; }
        public string schema { get; set; }
        public bool hidden { get; set; }
        public string metaData { get; set; }
        public string xsl { get; set; }
    }

    public class ObjectType
    {
        public string type { get; set; }
    }

    public class Method
    {
        public string objectName { get; set; }
        public List<ObjectType> objectTypes { get; set; }
        public string methodName { get; set; }
        public string methodtype { get; set; }
    }

    public class Settings
    {
        public Method method { get; set; }
    }

    public class Item
    {
        public List<object> itemProperties { get; set; }
    }

    public class Items
    {
        public Item item { get; set; }
    }

    public class ItemReference
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public bool primary { get; set; }
        public Settings settings { get; set; }
        public Items items { get; set; }
        public string xmlContent { get; set; }
    }

    public class Task
    {
        public string serialNumber { get; set; }
        public string status { get; set; }
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
        public List<CustomAction> customActions { get; set; }
        public List<string> comments { get; set; }
        public List<DataField> workflowInstanceDataFields { get; set; }
        public List<DataField> activityDataFields { get; set; }
        public List<XMLField> workflowInstanceXmlFields { get; set; }
        public List<ItemReference> itemReferences { get; set; }


        public Task()
        {
            customActions = new List<CustomAction>();
            comments = new List<string>();
            workflowInstanceDataFields = new List<DataField>();
            activityDataFields = new List<DataField>();
            workflowInstanceXmlFields = new List<XMLField>();
            itemReferences = new List<ItemReference>();

        }
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

        public List<CustomAction> customActions { get; set; }

        public TaskAction()
        {

            customActions = new List<CustomAction>();


        }
    }



    public class WorkflowInstances_Post
    {
        public string folio { get; set; }
        public int expectedDuration { get; set; }
        public int priority { get; set; }
        // public DataFields dataFields { get; set; }
        public List<XMLField> xmlFields { get; set; }
        public List<ItemReference> itemReferences { get; set; }

        public WorkflowInstances_Post()
        {
            //dataFields = new List<K2RestAPI.DataField>();
            xmlFields = new List<K2RestAPI.XMLField>();
            itemReferences = new List<ItemReference>();
        }
    }

    public class Tasks_Get
    {
        public int itemCount { get; set; }
        public List<Task> tasks { get; set; }
    }

    class K2API
    {

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
            String b64creds = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("admin@k2labs703.onmicrosoft.com:pass@word1"));
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
            // https://fzgbiqd.appitqa.com/api/workflow/preview/

            string sURL = string.Format("https://fzgbiqd.appitqa.com:443/Api/Workflow/preview/tasks");
            string webdata = WebGet(sURL);

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();
            Tasks_Get tg = js.Deserialize<Tasks_Get>(webdata);

            return tg.tasks;
        }

        //public bool finishTask()

        public string actionTask(string serial, TaskAction action) // TODO: Add optional username field 
        {

            string sURL = string.Format(String.Format("https://fzgbiqd.appitqa.com:443/Api/Workflow/preview/tasks/{0}", serial));

            System.Web.Script.Serialization.JavaScriptSerializer js = new JavaScriptSerializer();

            //construct JSON post data
            string postBody = string.Empty;
            if (null != action)
                postBody = js.Serialize(action);

            Console.WriteLine(postBody);

            string returnjson = WebPut(sURL, postBody);

            return returnjson;
        }

        public string startWorkflow(int workflowId, WorkflowInstances_Post wfi) // TODO: Add optional username field 
        {

            string sURL = string.Format(String.Format("https://fzgbiqd.appitqa.com:443/Api/Workflow/preview/workflows/{0}", workflowId));

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

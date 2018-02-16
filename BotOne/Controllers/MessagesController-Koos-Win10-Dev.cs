using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using System.Collections.Generic;


using XKParticle;
using K2RestAPI;


namespace BotOne
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private K2RestAPI.K2API k2api = new K2API();

        private static Dictionary<String, String> _ConversationKeys;
        private static Dictionary<String, String> _TokenDevices;

        
        private string startwf(int procid, string folio)
        {
            string retmsg = "New Process instance ";

            int piid = 0;

            WorkflowInstances_Post wfi = new WorkflowInstances_Post();
            wfi.folio = folio;
            piid = int.Parse(k2api.startWorkflow(procid, wfi));


            if (piid > 0)
                retmsg = string.Format("{0} started with InstanceID {1}", retmsg, piid);
            else
                retmsg = string.Format("NOT started! Please see server error log.", retmsg);

            return retmsg;
        }

        private string gettasks()
        {
            string retmsg = "You have the following tasks: \n\r";

            foreach (K2RestAPI.Task task in k2api.getTasks())
            {
                retmsg += "ID = " + task.serialNumber + ", Actions = |";
                foreach (K2RestAPI.CustomAction action in task.customActions)
                {
                    retmsg += action.name + "|";
                }
                retmsg += " [" + task.workflowInstanceFolio + "]\n\r";
            }

            return retmsg;
        }

        private string actiontask(string serial, string action)
        {
            string retmsg = "Task actioned! ";

            TaskAction ta = new TaskAction();
            CustomAction ca = new CustomAction();
            ca.batchable = false;
            ca.name = action;
            ta.customActions.Add(ca);

           retmsg += k2api.actionTask(serial, ta);

            return retmsg;
        }

        public MessagesController()
        {
            if (_ConversationKeys == null)
                _ConversationKeys = new Dictionary<string, string>();

            if (_TokenDevices == null)
                _TokenDevices = new Dictionary<string, string>();
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

           

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                Activity reply = activity.CreateReply($"Sorry, I did not understand your request?\n\rJust type \"Help\" if you need help.");

                String msgString = activity.Text.ToLower();
    

                if (msgString.Contains("help"))
                {
                    string replystring = "I can understand the following instructions:\n\r";
                    replystring += "\"Start Process\", to kick off a new K2 process.\n\r";
                    replystring +=  "\"Get Tasks\", to get your task list.\n\r";
                    replystring += "\"Execute Action=[ActionName] with task=[SerialNo]\", to action a task .\n\r";
                    reply = activity.CreateReply(replystring);

                }
                else if (msgString.Contains("id"))
                {
                    reply = activity.CreateReply($"ID: {activity.From.Id}");
                }
                else if ((msgString.Contains("action=")) || (msgString.Contains("task=")))
                {
                    if ((msgString.Contains("action=")) && (msgString.Contains("task=")))
                    {
                        int aloc = msgString.IndexOf("action=") + 7; //start index of action value
                        int alen = msgString.Substring(aloc).IndexOf(" ");
                        string action = (alen>0)? msgString.Substring(aloc,alen):msgString.Substring(aloc);

                        int tloc = msgString.IndexOf("task=") + 5; //start index of task value
                        int tlen = msgString.Substring(tloc).IndexOf(" ");
                        string task = (tlen > 0) ? msgString.Substring(tloc, tlen) : msgString.Substring(tloc);

                        reply = activity.CreateReply(actiontask(task, action));

                    }
                    else
                        reply = activity.CreateReply("Please specify both task=xxx and action=xxx");
                }
                else if (msgString.Contains("folio="))
                {
                    int pid = 29; //procid of process to start..
                    int floc = msgString.IndexOf("folio=") + 6; //start index of Folio value
                    string folio = activity.Text.Substring(floc);

                    reply = activity.CreateReply(startwf(pid, folio));
                }
                else if (msgString.Contains("tasks"))
                {
                    reply = activity.CreateReply(gettasks());
                }
                else if (msgString.Contains("finish"))
                {
                    reply = activity.CreateReply($"Thanks for your approval,  {activity.From.Name}.\nThis task is now complete.");
                }
                else if (msgString.Contains("set token="))
                {
                    String tokenvalue = msgString.Substring(10, msgString.Length - 10);
                    _ConversationKeys[activity.Conversation.Id] = tokenvalue;
                    reply = activity.CreateReply($"Setting your Token Value to: {tokenvalue}");
                }
                else if (msgString.Contains("set device="))
                {
                    if (_ConversationKeys.ContainsKey(activity.Conversation.Id))
                    {
                        String deviceName = msgString.Substring(11, msgString.Length - 11);
                        _TokenDevices[activity.Conversation.Id] = deviceName;
                        reply = activity.CreateReply($"Setting your active Device to: {deviceName}");
                    }
                    else
                    {
                        reply = activity.CreateReply($"Sorry, your token value has not been set yet.");
                    }
                }
                else if (msgString.Contains("get token"))
                {
                    if (_ConversationKeys.ContainsKey(activity.Conversation.Id))
                    {
                        String tokenvalue = _ConversationKeys[activity.Conversation.Id];
                        reply = activity.CreateReply($"Your Token Value is: {tokenvalue}");
                    }
                    else
                    {
                        reply = activity.CreateReply($"Sorry, your token value has not been set yet.");
                    }
                }
                else if (msgString.Contains("device list"))
                {
                    if (_ConversationKeys.ContainsKey(activity.Conversation.Id))
                    {
                        String tokenvalue = _ConversationKeys[activity.Conversation.Id];

                        String replyString = "Your Device list:";

                        ParticleAPI particle = new ParticleAPI(tokenvalue);
                        List <ParticleDevice> devices = particle.GetDeviceList();
                        foreach(ParticleDevice device in devices)
                        {
                            replyString += $"\n\r  {device.name} - {device.id} ";
                            if (device.connected)
                                replyString += " (Online)";
                        }

                        reply = activity.CreateReply(replyString);
                    }
                    else
                    {
                        reply = activity.CreateReply($"Sorry, your token value has not been set yet.");
                    }
                }
                else if (msgString.Contains("method list"))
                {
                    if (_ConversationKeys.ContainsKey(activity.Conversation.Id))
                    {
                        String tokenvalue = _ConversationKeys[activity.Conversation.Id];

                        if (_TokenDevices.ContainsKey(activity.Conversation.Id))
                        {
                            String deviceID = _TokenDevices[activity.Conversation.Id];

                            String replyString = "Method List: ";

                            ParticleAPI particle = new ParticleAPI(tokenvalue);

                            ParticleDevice device = particle.GetDeviceDetails(deviceID);

                            if (device.functions.Count > 0)
                            {
                                foreach (string funcname in device.functions)
                                {
                                    replyString += $"\n\r  {funcname}";

                                }
                            }
                            else
                            {
                                replyString = "No Methods published for this device";
                            }

                            reply = activity.CreateReply(replyString);
                        }
                        else
                        {
                            reply = activity.CreateReply($"Sorry, your active Device has not been set yet.");
                        }
                    }
                    else
                    {
                        reply = activity.CreateReply($"Sorry, your token value has not been set yet.");
                    }
                }
                else if (msgString.Contains("call method"))
                {
                    String methodName = msgString.Substring(12, msgString.Length - 12);
                    String tokenvalue = _ConversationKeys[activity.Conversation.Id];
                    String deviceID = _TokenDevices[activity.Conversation.Id];

                    ParticleAPI particle = new ParticleAPI(tokenvalue);
                    particle.CallDeviceFunction(deviceID, methodName, "bar");

                    reply = activity.CreateReply($"Calling Method: {methodName}");
                }
                else if (msgString.Contains("hello") || msgString.CompareTo("hi") == 0)
                {
                    reply = activity.CreateReply($"Greetings {activity.From.Name}, I am K2 bot v0.1.6.");
                }

                //Send Response..
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
          

            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channell

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }



            return null;
        }
    }
}
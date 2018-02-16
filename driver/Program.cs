using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace driver
{
    class Program
    {
        static void Main(string[] args)
        {
        
           K2RestAPI.K2API k2api = new K2RestAPI.K2API();

            string retmsg = "You have the following tasks: \n\r";

            foreach (K2RestAPI.Task task in k2api.getTasks())
            {
                retmsg += "ID = " + task.serialNumber + ", Actions = |";
                foreach (string action in task.actions.batchableActions)
                {
                    retmsg += action + "|";
                }
                retmsg += " [" + task.workflowInstanceFolio + "]\n\r";
            }
            Console.WriteLine(retmsg);

            retmsg = "Task actioned! ";
            retmsg += k2api.actionTask("10219_73", "approve");

            Console.WriteLine(retmsg);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ClipIndustry
{
    /// <summary>
    /// Allows retrieval of data from SH_Game through the network for the client GUI. Also allows for the user to send commands to the
    /// game server.
    /// </summary>
    [Serializable]
    internal class SH_ClientNetworking
    {
        //Initializer
        public SH_ClientNetworking(string serverURI)
        {
            targetServerURI = serverURI;

            //Try Connecting to server
            HttpClient client = new HttpClient();
            try
            {
                Console.WriteLine("CL_NET: Attempting to contact server...");
                HttpResponseMessage response = client.GetAsync(targetServerURI + "statuscheck/").Result;
                Console.WriteLine((response.IsSuccessStatusCode ? "" : "(FATAL) ") + "CL_NET: Server connection " + (response.IsSuccessStatusCode ? "OK" : "BAD") + " (" + response.StatusCode + ")");
                Console.WriteLine("CL_NET: Date Time: " + response.Content.ReadAsStringAsync().Result + " (" + DateTime.Now.Subtract(DateTime.Parse(response.Content.ReadAsStringAsync().Result)).TotalMilliseconds + " ms)");
            }
            catch (System.AggregateException)
            {
                Console.WriteLine("(FATAL) CL_NET: Failure to contact server.");
            }
        }

        //Attempt to reach the given URL and make sure it responds like the server would
        public bool checkServerConnectivity()
        {
            bool success = true;
            //Try Connecting to server
            HttpClient client = new HttpClient();
            try
            {
                Console.WriteLine("CL_NET: Attempting to contact server...");
                HttpResponseMessage response = client.GetAsync(targetServerURI + "statuscheck/").Result;
                Console.WriteLine((response.IsSuccessStatusCode ? "" : "(FATAL) ") + "CL_NET: Server connection " + (response.IsSuccessStatusCode ? "OK" : "BAD") + " (" + response.StatusCode + ")");
                Console.WriteLine("CL_NET: Date Time: " + response.Content.ReadAsStringAsync().Result + " (" + DateTime.Now.Subtract(DateTime.Parse(response.Content.ReadAsStringAsync().Result)).TotalMilliseconds + " ms)");
            }
            catch (System.AggregateException)
            {
                Console.WriteLine("(FATAL) CL_NET: Failure to contact server.");
                success = false;
            }

            return success;
        }

        //Request a complete version of the game context from the server. (Takes a while, use differential).
        public SH_GameContext RequestContextFromServer()
        {
            using(HttpClient client = new HttpClient())
            {
                try
                {
                    Stream stream = client.GetStreamAsync(targetServerURI + "con/whole/").Result;
                    IFormatter formatter = new BinaryFormatter();
                    return (SH_GameContext)formatter.Deserialize(stream);
                }
                catch (System.AggregateException)
                {
                    //"possible" null reference return
                    return null;
                }
            }
        }

        private string targetServerURI;
    }
}

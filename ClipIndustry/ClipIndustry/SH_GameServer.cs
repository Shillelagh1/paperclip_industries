using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace ClipIndustry
{
    /// <summary>
    /// Allows the clients to interact with SH_GameContext over the network.
    /// </summary>
    class SH_GameServer
    {
        public uint serverPort = 8080;

        public SH_GameContext gameContext;

        public SH_GameServer(SH_GameContext context)
        {
            gameContext = context;
        }

        //Construct the network object
        public void SH_GameServerInitialize(uint port)
        {
            //Make sure that HttpListener is actually supported
            if (HttpListener.IsSupported)
            {
                Console.WriteLine("SV_NET: HTTPListener supported, server good to go!");
            }
            else
            {
                Console.WriteLine("SV_NET: HTTPListener is not supported.");
            }

            //Start listening
            SH_StartListener(String.Format("http://*:{0}/", port));
        }

        public void SH_StartListener(string prefix)
        {
            var t = new Thread(() => SH_ListenerThread(prefix));
            t.Start();
        }

        /// <summary>
        /// A 
        /// </summary>
        /// <param name="prefix"></param>
        private void SH_ListenerThread(string prefix)
        {
            //Start the listener
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine("SV_NET: " + prefix + " Listening on port " + serverPort);

            while (true)
            {
                //Get request
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                Console.WriteLine("");
                Console.WriteLine("SV_NET: Handling " + request.HttpMethod + " request: " + request.RawUrl);

                //Set up response
                int responseStatusCode = 200;
                if(request.RawUrl == null)
                {
                    continue;
                }
                string responseString = handleGETRequest(request.RawUrl, ref responseStatusCode);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                //Send Response
                //If the response code is 999, we want to send a Stream, not a string!
                //This is in case we are sending entire classes from the context
                //TODO: Implement asking context for the stream.
                //For now we just bundle up context as a whole, and send it over. Why not?
                Stream output;
                if(responseStatusCode == 999)
                {
                    IFormatter formatter = new BinaryFormatter();
                    output = response.OutputStream;
                    formatter.Serialize(output, gameContext);
                }
                else
                {
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = responseStatusCode;
                    output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                }
                output.Close();
            }
        }

        private string handleGETRequest(string RawURL, ref int replyStatusCode)
        {
            //In case we try to access it as a webpage.
            if (RawURL == "/")
            {
                return "<HTML><BODY><b>Server is running.</b><br>Please launch the game client to connect to the server.</BODY></HTML>";
            }

            string commandTarget = RawURL.Substring(1, Math.Max(RawURL.IndexOf("/", 1) - 1, 0));
            string command = RawURL.Substring(commandTarget.Length + 2);
            switch (commandTarget)
            {
                default:
                    return "Invalid Command";

                case "statuscheck":
                    return DateTime.Now.TimeOfDay.ToString();

                case "con":
                    return gameContext.HandleNetworkCommand(command, ref replyStatusCode);

                case "register":
                    string[] arguments = command.Split(',');
                    if (arguments.Length < 3)
                    {
                        return "SV_NET: Could not create user, too few args.";
                    }
                    return createUser(arguments[0], arguments[1], arguments[2], ref replyStatusCode);
            }
        }

        private string createUser(string shortName, string userName, string userPassk, ref int replyStatusCode)
        {
            if (gameContext.UserExists(shortName.ToUpper()))
            {
                replyStatusCode = 406;
                return "SV_NET: Cannot create user, it already exists";
            }

            createUserListener(shortName);
            gameContext.RegisterUser(shortName.ToUpper(), userName, userPassk);
            replyStatusCode = 201;
            return "SV_NET: User Created :D";
        }

        public void createUserListener(string shortName)
        {
            SH_StartListener(String.Format("http://*:{0}/con/u'{1}'/", serverPort, shortName));
        }
    }
}
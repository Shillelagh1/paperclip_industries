using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace ClipIndustry
{
    //class for information about every players
    [Serializable]
    class SH_GameContext_User
    {
        string userLogin;
        string userPassK;
        public SH_GameContext_User(string login, string passK)
        {
            userLogin = login;
            userPassK = passK;
        }
    }

    /// <summary>
    /// Provides context for the game (SH_Game). Provides authentication and allows users to interact with the game.
    /// </summary>
    [Serializable]
    class SH_GameContext
    {

        //All players
        Dictionary<string, SH_GameContext_User> gameUsers;

        public SH_Game game;

        //Initializer
        /// <summary>
        /// I BARELY KNEW HER.
        /// </summary>
        public SH_GameContext()
        {
            gameUsers = new Dictionary<string, SH_GameContext_User>();
            game = new SH_Game();
        }

        /// <summary>
        /// Handle a context command from the game server.
        /// </summary>
        /// <param name="commandWithArgs"></param>
        /// <param name="replyStatusCode"></param>
        /// <returns></returns>
        public string HandleNetworkCommand(string commandWithArgs, ref int replyStatusCode)
        {
            //IS USER SPECIFIC
            if (Regex.IsMatch(commandWithArgs, @"^u'[A-Z][A-Z]'"))
            {
                string userShortName = commandWithArgs.Split('/')[0].Substring(2, 2);
                if (UserExists(userShortName))
                {
                    return String.Format("CON/{0}: Invalid Command", userShortName);
                }
                replyStatusCode = 401;
                return "CON/##: Invalid User";
            }
            //IS NOT USER SPECIFIC
            else
            {
                string[] commandParts = commandWithArgs.Split('/');
                string[] commandArgs = new string[] { };
                if(commandParts.Length > 1) 
                {
                    commandArgs = commandParts[1].Split(',');
                }
                
                switch (commandParts[0])
                {
                    default:
                        return "CON: Invalid Command";

                    case "save":
                        if (commandArgs.Length > 0)
                        {
                            saveContext(HttpUtility.UrlDecode(commandArgs[0]).Replace('|', '\\'));
                            return "CON: Saved!";
                        }
                        return "CON: Too few arguments to save file";

                    case "todo":
                        return "<HTML><BODY>" + todoList() + "</HTML></BODY>";

                    case "whole":
                        replyStatusCode = 999;
                        return "AWAIT SUITABLE COMMAND!";
                }
            }
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="userName"></param>
        /// <param name="passK"></param>
        public void RegisterUser(string shortName, string userName, string passK)
        {
            gameUsers.Add(shortName, new SH_GameContext_User(userName, passK));
        }

        /// <summary>
        /// Returns if a user already exists.
        /// </summary>
        /// <param name="shorName"></param>
        /// <returns></returns>
        public bool UserExists(string shorName)
        {
            return gameUsers.ContainsKey(shorName);
        }

        /// <summary>
        /// Saves the game context to a file
        /// </summary>
        /// <param name="fileName"></param>
        public void saveContext(string fileName)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            //TODO: This is obselete, fix later
            formatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Load the game context from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static public SH_GameContext loadContext(string fileName)
        {
            //TODO (URGENT): Make sure the file exists?
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            IFormatter formatter = new BinaryFormatter();
            //TODO: This is obselete, fix later
            SH_GameContext context = (SH_GameContext)formatter.Deserialize(stream);
            stream.Close();
            return context;
        }

        /// <summary>
        /// Create a listener for every user (To be used after loading).
        /// </summary>
        /// <param name="gameServer"></param>
        public void startAllUserListeners(SH_GameServer gameServer)
        {
            foreach (string iUser in gameUsers.Keys)
            {
                gameServer.createUserListener(iUser);
            }
        }

        public string todoList(string newLine = "<br>")
        {
            string todo = "";
            //Make checks and add todo items here

            if (gameUsers.Count < 1)
            {
                todo += "Warning: There should be at least one user in the game. Users can be added later." + newLine;
            }

            if (game == null)
            {
                todo += "Fatal: There is no game. Create one";
            }

            if (todo == "")
            {
                todo += "Nothing, good to go!" + newLine;
            }
            return todo;
        }
    }
}

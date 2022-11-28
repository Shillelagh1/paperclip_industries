using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

#pragma warning disable CS8604 //SHUT THE FUCK UP OH MY GOD
namespace ClipIndustry
{
    class Program
    {
        static Regex yRegex = new Regex(@"^y.*$");

        static void Main(string[] args) 
        {
            Console.WriteLine("Initialize server? (y/n)");
            bool isServer = yRegex.IsMatch(stringNonNullable(Console.ReadLine()).ToLower());
            SH_GameContext gameContext;

            if (isServer)
            {
                //Game Context
                Console.Write("Load file? (y/n): ");
                Console.WriteLine("");
                if (yRegex.IsMatch(stringNonNullable(Console.ReadLine())))
                {
                    //Load the game
                    Console.Write("File path to load: ");
                    gameContext = SH_GameContext.loadContext(stringNonNullable(Console.ReadLine()));
                    Console.WriteLine("\n");
                }
                else
                {
                    //Just create a new game
                    gameContext = new SH_GameContext();
                }

                //Server Networking
                SH_GameServer gameServer = new SH_GameServer(gameContext);
                Console.WriteLine("Attempting to start server...");
                gameServer.SH_GameServerInitialize(8080);

                //Start all users networking on the game server (In case we loaded)
                gameContext.startAllUserListeners(gameServer);
                gameContext.game.gameMaps = new SH_Map[]
                {
                    SH_Map.newMapFromText("D:\\Documents\\Clip\\mapEarth.txt")
                };
                gameContext.game.name = "nut dude.EPG";
            }

            Task.Delay(1000);

            Console.WriteLine("\nInitialize client? (y/n)");
            bool isClient = yRegex.IsMatch(stringNonNullable(Console.ReadLine()).ToLower());

            if (isClient)
            {
                SH_ClientNetworking networking = new SH_ClientNetworking("http://localhost:8080/");
                Console.WriteLine(networking.RequestContextFromServer().game.gameMaps[0].mapTiles.Length);

            }

            Console.Read();
        }

        /// <summary>
        /// To please the .NET 6.0 gods.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string stringNonNullable(string str)
        {
            return str == null ? "" : str;
        }
    }
}

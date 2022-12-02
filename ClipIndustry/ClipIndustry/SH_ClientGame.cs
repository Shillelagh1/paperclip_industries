using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace ClipIndustry
{
    internal class SH_ClientGame
    {
        public SH_ClientNetworking networking;
        public SH_GameContext context;

        bool gameRun;
        bool gameHalt = false;

        //SDL2 stuff
        public IntPtr window;
        public IntPtr renderer;
        public IntPtr propertyWindow;
        public IntPtr propertyRenderer;

        //Page things
        SH_CLMainWindow_Map nwindow = null;
        public enum MainPages
        {
            page_map,
            page
        }

        public enum PropertyMenus
        {
            map,
            tile_inspect,
            crafting
        }

        Dictionary<MainPages, SH_CLMainWindow> pages = new Dictionary<MainPages, SH_CLMainWindow>();
        Dictionary<PropertyMenus, SH_CLPropertyWindow> properties = new Dictionary<PropertyMenus, SH_CLPropertyWindow>();

        public MainPages selectedPage = MainPages.page_map;
        public PropertyMenus selectedProperty = PropertyMenus.map;

        /// <summary>
        /// Constructor. Starts the game as well
        /// </summary>
        /// <param name="_networking"></param>
        public SH_ClientGame(SH_ClientNetworking _networking)
        {
            //Establish a network connection
            networking = _networking;
            gameRun = true;
            nwindow = new SH_CLMainWindow_Map();
            //Make sure we can connect to the server on the first try
            //TODO: Give the server multiple attempts to respond and
            //allow the client to retry again later without restarting the application.
            if (networking.checkServerConnectivity())
            {
                //We run the game in a separate thread. Start that thread
                Thread _gameThread = new Thread(GameUpdateThread);
                _gameThread.Start();
            }
        }

        /// <summary>
        /// Runs the client GUI and makes network requests. Put this in its own thread
        /// </summary>
        private void GameUpdateThread()
        {
            setupSDL();

            setupPagesAndProperties();

            context = networking.RequestContextFromServer();

            while (gameRun)
            {
                //Halt before running the tick if need be
                //Don't halt if we shutdown please?
                while (gameHalt && gameRun)
                {
                    Console.WriteLine("HALT");
                }

                //Handle poll events for SDL's highness
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            gameRun = false;
                            break;
                    }

                    if (SDL.SDL_GetMouseFocus() == window)
                    {
                        if (!pages[selectedPage].handleEvent(e, this))
                        {

                        }
                    }
                }

                updatePage();
                updateProperty();
            }
        }

        /// <summary>
        /// Render the currently selected page.
        /// </summary>
        public void updatePage()
        {
            //Update the selected main page.
            pages[selectedPage].updateWindow(this);
        }

        public void updateProperty()
        {
            properties[selectedProperty].updateProperty(this);
        }

        /// <summary>
        /// Setup SDL and create a window
        /// </summary>
        public void setupSDL()
        {
            //Start main window and stuff
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow("Paperclip INDUSTRIES", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 800, 500, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            renderer = SDL.SDL_CreateRenderer(window,
                                        -1,
                                        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                        SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            //Property window
            propertyWindow = SDL.SDL_CreateWindow("Properties", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 300, 500, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            propertyRenderer = SDL.SDL_CreateRenderer(propertyWindow,
                                        -1,
                                        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                        SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG | SDL_image.IMG_InitFlags.IMG_INIT_JPG);
        }

        /// <summary>
        /// Populate the dictionaries with all the necessary values
        /// </summary>
        void setupPagesAndProperties()
        {
            //Setup pages
            pages.Add(MainPages.page, new SH_CLMainWindow());
            pages.Add(MainPages.page_map, new SH_CLMainWindow_Map());

            //Setup properties
            properties.Add(PropertyMenus.map, new SH_CLPropertyWindow());
            properties.Add(PropertyMenus.tile_inspect, new SH_CLPropertyWindow_TileInspect());
        }
    }
}

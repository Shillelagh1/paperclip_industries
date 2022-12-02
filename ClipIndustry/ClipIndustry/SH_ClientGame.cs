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
        SH_ClientNetworking networking;
        SH_GameContext context;

        bool gameRun;
        bool gameHalt = false;

        //SDL2 stuff
        IntPtr window;
        IntPtr renderer;
        IntPtr propertyWindow;
        IntPtr propertyRenderer;

        //Map stuff
        struct TileInfo
        {
            public int _x;
            public int _y;
        }
        TileInfo hoveredTile = new TileInfo();
        TileInfo selectedTile = new TileInfo();
        SH_MapRegion focusRegion = null;
        int tilePixelHeight = 5;
        int tilePixelWidth = 4;

        //Page things
        enum SelectedPage
        {
            page_map
        }

        enum SelectedPropertyMenu
        {
            map,
            tile_inspect,
            crafting
        }

        SelectedPage selectedPage = SelectedPage.page_map;
        SelectedPropertyMenu selectedProperty = SelectedPropertyMenu.map;

        /// <summary>
        /// Constructor. Starts the game as well
        /// </summary>
        /// <param name="_networking"></param>
        public SH_ClientGame(SH_ClientNetworking _networking)
        {
            //Establish a network connection
            networking = _networking;
            gameRun = true;

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
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym)
                            {
                                case SDL.SDL_Keycode.SDLK_t:
                                    if(SDL.SDL_GetModState() == SDL.SDL_Keymod.KMOD_LCTRL)
                                    {
                                        selectedProperty = SelectedPropertyMenu.crafting;
                                    }
                                    break;
                            }
                            break;
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
            //Render the main page
            switch (selectedPage)
            {
                default:
                    SDL.SDL_SetRenderDrawColor(renderer, 204, 230, 255, 255);

                    SDL.SDL_RenderClear(renderer);

                    SDL.SDL_RenderPresent(renderer);
                    break;

                case SelectedPage.page_map:
                    #region
                    //Render
                    SDL.SDL_SetRenderDrawColor(renderer, 204, 230, 255, 255);

                    SDL.SDL_RenderClear(renderer);

                    //TODO: add support for more maps?

                    int selectedMapIndex = 0;
                    SH_Map selectedMap = context.game.gameMaps[selectedMapIndex];

                    SDL.SDL_Rect _rect = new SDL.SDL_Rect();
                    _rect.x = 0;
                    _rect.y = 0;
                    _rect.w = tilePixelWidth;
                    _rect.h = tilePixelHeight;
                    for(int x = 0; x < selectedMap.mapWidth - 1; x++)
                    {
                        for (int y = 0; y < selectedMap.mapHeight -1; y++)
                        {
                            SH_MapTile _tile = selectedMap.mapTiles[x, y];
                            _rect.x = x * tilePixelWidth;
                            _rect.y = y * tilePixelHeight;

                            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

                            if(selectedMap.isValidPosition(hoveredTile._x, hoveredTile._y))
                            {
                                //Calculate if the region highlight is applied. World's longest line?
                                bool highlight = (DateTime.Now.Second % 2 == 0) && selectedMap.mapTiles[hoveredTile._x, hoveredTile._y].region.regionMapTiles.Contains(_tile);

                                if (hoveredTile._x == x && hoveredTile._y == y)
                                {
                                    SDL.SDL_SetRenderDrawColor(renderer, 255, 240, 40, 255);
                                }
                                else if (highlight)
                                {
                                    SDL.SDL_SetRenderDrawColor(renderer, 255, 240, 40, 64);
                                }
                            }

                            if (_tile.region.regionIdentifier != "X") SDL.SDL_RenderFillRect(renderer, ref _rect);
                        }
                    }
                    SDL.SDL_RenderPresent(renderer);

                    //Tile Selection
                    if(SDL.SDL_GetMouseFocus() == window)
                    {
                        int _mouseX = 0;
                        int _mouseY = 0;

                        SDL.SDL_GetMouseState(out _mouseX, out _mouseY);

                        int _slX = (int)MathF.Floor(_mouseX / tilePixelWidth);
                        int _slY = (int)MathF.Floor(_mouseY / tilePixelHeight);

                        hoveredTile._x = _slX;
                        hoveredTile._y = _slY;

                        if(selectedMap.isValidPosition(_slX, _slY))
                        {
                            focusRegion = selectedMap.mapTiles[_slX, _slY].region;
                        }
                    }
                    #endregion
                    break;
            }
        }

        public void updateProperty()
        {
            switch (selectedProperty)
            {
                default:
                    SDL.SDL_SetRenderDrawColor(propertyRenderer, 204, 230, 255, 255);

                    SDL.SDL_RenderClear(propertyRenderer);

                    SDL.SDL_RenderPresent(propertyRenderer);
                    break;
                case SelectedPropertyMenu.crafting:

                    break;
            }
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
    }
}

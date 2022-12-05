using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace ClipIndustry
{
    internal class SH_CLMainWindow
    {
        public virtual void updateWindow(SH_ClientGame _client)
        {
            //Just clear the screen lol
            SDL.SDL_SetRenderDrawColor(_client.renderer, 204, 230, 255, 255);

            SDL.SDL_RenderClear(_client.renderer);

            SDL.SDL_RenderPresent(_client.renderer);
        }

        public virtual bool handleEvent(SDL.SDL_Event e, SH_ClientGame _client)
        {
            return false;
        }
    }

    /// <summary>
    /// Map window.
    /// </summary>
    internal class SH_CLMainWindow_Map : SH_CLMainWindow
    {
        public struct TileInfo
        {
            public int _x;
            public int _y;
        }
        public TileInfo hoveredTile = new TileInfo();
        public TileInfo selectedTile = new TileInfo();
        public SH_MapRegion focusRegion = null;
        public int tilePixelHeight = 5;
        public int tilePixelWidth = 4;

        public override void updateWindow(SH_ClientGame _client)
        {
            //Render
            SDL.SDL_SetRenderDrawColor(_client.renderer, 204, 230, 255, 255);

            SDL.SDL_RenderClear(_client.renderer);

            //TODO: add support for more maps?

            int selectedMapIndex = 0;
            SH_Map selectedMap = _client.context.game.gameMaps[selectedMapIndex];

            SDL.SDL_Rect _rect = new SDL.SDL_Rect();
            _rect.x = 0;
            _rect.y = 0;
            _rect.w = tilePixelWidth;
            _rect.h = tilePixelHeight;
            for (int x = 0; x < selectedMap.mapWidth - 1; x++)
            {
                for (int y = 0; y < selectedMap.mapHeight - 1; y++)
                {
                    SH_MapTile _tile = selectedMap.mapTiles[x, y];
                    _rect.x = x * tilePixelWidth;
                    _rect.y = y * tilePixelHeight;

                    SDL.SDL_SetRenderDrawColor(_client.renderer, 255, 0, 0, 255);

                    if (selectedMap.isValidPosition(hoveredTile._x, hoveredTile._y))
                    {
                        //Calculate if the region highlight is applied. World's longest line?
                        //Should cause all tiles with the region
                        bool highlight = (DateTime.Now.Second % 2 == 0) && selectedMap.mapTiles[hoveredTile._x, hoveredTile._y].region.regionMapTiles.Contains(_tile);

                        if (hoveredTile._x == x && hoveredTile._y == y)
                        {
                            SDL.SDL_SetRenderDrawColor(_client.renderer, 255, 240, 40, 255);
                        }
                        else if (selectedTile._x == x && selectedTile._y == y && DateTime.Now.Millisecond % 300 < 150)
                        {
                            SDL.SDL_SetRenderDrawColor(_client.renderer, 204, 240, 255, 255);
                        }
                        else if (highlight)
                        {
                            SDL.SDL_SetRenderDrawColor(_client.renderer, 255, 190, 130, 255);
                        }
                    }

                    if (_tile.region.regionIdentifier != "X") SDL.SDL_RenderFillRect(_client.renderer, ref _rect);
                }
            }

            SDL.SDL_RenderPresent(_client.renderer);

            //Tile Selection
            if (SDL.SDL_GetMouseFocus() == _client.window)
            {
                int _mouseX = 0;
                int _mouseY = 0;

                SDL.SDL_GetMouseState(out _mouseX, out _mouseY);

                int _slX = (int)MathF.Floor(_mouseX / tilePixelWidth);
                int _slY = (int)MathF.Floor(_mouseY / tilePixelHeight);

                hoveredTile._x = _slX;
                hoveredTile._y = _slY;

                if (selectedMap.isValidPosition(_slX, _slY))
                {
                    focusRegion = selectedMap.mapTiles[_slX, _slY].region;
                }
            }
        }

        public override bool handleEvent(SDL.SDL_Event e, SH_ClientGame _client)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    switch (e.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_t:
                            if (SDL.SDL_GetModState() == SDL.SDL_Keymod.KMOD_LCTRL)
                            {

                            }
                            break;
                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if(e.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        selectedTile = hoveredTile;
                        _client.selectedProperty = SH_ClientGame.PropertyMenus.tile_inspect;
                    }
                    break;
            }
            return true;
        }
    }

    internal class SH_CLPropertyWindow
    {
        public virtual void updateProperty(SH_ClientGame _client)
        {

        }
    }

    internal class SH_CLPropertyWindow_TileInspect : SH_CLPropertyWindow
    {
        public override void updateProperty(SH_ClientGame _client)
        {
            SDL.SDL_Rect _rect;
            _rect.x = 0;
            _rect.y = 0;
            _rect.w = 30;
            _rect.h = 30;
            SDL.SDL_SetRenderDrawColor(_client.propertyRenderer, (byte)((DateTime.Now.Millisecond % 300 < 150) ? 255 : 128), 0, 0, 255);
            SDL.SDL_RenderFillRect(_client.propertyRenderer, ref _rect);
            SDL.SDL_RenderPresent(_client.propertyRenderer);
        }
    }
}

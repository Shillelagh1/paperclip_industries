using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace ClipIndustry
{
    internal class SH_Rendering
    {
        public SH_Rendering()
        {

        }
    }

    internal class SheetTexture
    {
        private IntPtr sheetTexture;
        public SDL.SDL_Rect[] spriteClips;
        int width;
        int height;

        public void render(IntPtr renderer, int xPos, int yPos, int clipToRender)
        {
            SDL.SDL_Rect renderQuad;
            renderQuad.x = xPos;
            renderQuad.y = yPos;
            renderQuad.w = width;
            renderQuad.h = height;
            
            if(0 <= clipToRender && clipToRender < spriteClips.Length)
            {
                renderQuad.w = spriteClips[clipToRender].w;
                renderQuad.h = spriteClips[clipToRender].h;
            }

            SDL.SDL_RenderCopy(renderer, sheetTexture, ref spriteClips[clipToRender], ref renderQuad);
        }

        public void loadFromFile(string path, IntPtr renderer)
        {
            IntPtr datasize;
            IntPtr surface = SDL_image.IMG_Load(path);

        }
    }
}

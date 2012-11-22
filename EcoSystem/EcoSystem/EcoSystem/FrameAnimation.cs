using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace EcoSystem
{
    class FrameAnimation : SpriteManager
    {
        public FrameAnimation(Texture2D Texture, int frames)
            : base(Texture, frames)
        {

        }

        public void SetFrame(int frame)
        {
            if (frame < Rectangles.Length)
                FrameIndex = frame;
        }

        public void nextFrame()
        {
            if (FrameIndex < base.Rectangles.Length-1)
            {
                FrameIndex++;
            }
            else
            {
                FrameIndex = 0;
            }
        }
    }
}

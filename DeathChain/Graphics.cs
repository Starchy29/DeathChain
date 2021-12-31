using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    // container for all visual assets. These should only be set by the Game1 class
    static class Graphics
    {
        public static SpriteFont Font { get; set; }

        public static Texture2D Pixel { get; set; }
        public  static Texture2D TempGhost { get; set; }
        public static Texture2D[] Mushroom { get; set; } // shoot animation, freezes on first frame when idle
        public static Texture2D Spore { get; set; }
        public static Texture2D[] SporeBurst { get; set; }
    }
}

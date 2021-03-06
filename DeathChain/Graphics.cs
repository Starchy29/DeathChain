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
        public static SpriteFont TitleFont { get; set; }

        public static Texture2D Pixel { get; set; }

        public static Texture2D[] PlayerFront { get; set; }
        public static Texture2D[] PlayerSide { get; set; }
        public static Texture2D[] PlayerBack { get; set; }
        public static Texture2D[] SlashEffect { get; set; }
        public static Texture2D[] PlayerForwardSlash { get; set; } // 90x90

        public static Texture2D[] Mushroom { get; set; } // shoot animation, freezes on first frame when idle
        public static Texture2D[] MushroomHide { get; set; } // shoot animation, freezes on first frame when idle
        public static Texture2D Spore { get; set; }
        public static Texture2D[] SporeBurst { get; set; }
        public static Texture2D[] SporeTrail { get; set; }
        public static Texture2D[] SporeBreak { get; set; }

        public static Texture2D Zombie { get; set; }

        public static Texture2D Slime { get; set; }
        public static Texture2D SlimeBall { get; set; }
        public static Texture2D[] SlimePuddle { get; set; }

        public static Texture2D Scarecrow { get; set; }
        public static Texture2D[] SpiralFlame { get; set; }
        public static Texture2D[] FlameBurst { get; set; }

        public static Texture2D Blight { get; set; }
        public static Texture2D[] BlightExplosion { get; set; }
        public static Texture2D[] BlightDissipate { get; set; }

        public static Texture2D Beast { get; set; }

        public static Texture2D Slash { get; set; }
        public static Texture2D Button { get; set; }
        public static Texture2D Dash { get; set; }
        public static Texture2D SporeLogo { get; set; }
        public static Texture2D Shield { get; set; }
        public static Texture2D Lunge { get; set; }
        public static Texture2D Possess { get; set; }
        public static Texture2D Unpossess { get; set; }
        public static Texture2D Soul { get; set; }
        public static Texture2D Heart { get; set; }
        public static Texture2D Drop { get; set; }
        public static Texture2D ExplosionLogo { get; set; }
        public static Texture2D DeathClock { get; set; }

        public static Texture2D PoisonPit { get; set; }

        // draws an image at the specified location, but rotates at that position
        public static void RotateDraw(SpriteBatch sb, Texture2D sprite, Rectangle location, Color color, float radians, SpriteEffects flips = SpriteEffects.None) {
            location.Offset(location.Width / 2, location.Height / 2);
            sb.Draw(sprite, location, null, color, radians, new Vector2(sprite.Width / 2f, sprite.Height / 2f), flips, 1f);
        }
    }
}

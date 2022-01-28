using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    static class Camera
    {
        private static Vector2 position; // top left

        public static Vector2 Shift { get { return -position; } }

        public static void Start() {
            position = new Vector2(0, 0);
        }

        public static void Update(Level level) {
            // center player in window
            position = Game1.Player.Focus - new Vector2(800, 450); // screen is 1600 by 900

            // keep camera in level
            Rectangle tangle = level.Bounds;
            if(position.X < tangle.X) {
                position.X = tangle.X;
            }
            if(position.X + Game1.StartScreenWidth > tangle.Right) {
                position.X = tangle.Right - Game1.StartScreenWidth;
            }
            if(position.Y < tangle.Y) {
                position.Y = tangle.Y;
            }
            if(position.Y + Game1.StartScreenHeight > tangle.Bottom) {
                position.Y = tangle.Bottom - Game1.StartScreenHeight;
            }
        }
    }
}

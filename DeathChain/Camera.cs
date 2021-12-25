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
            position = Game1.Player.Midpoint - new Vector2(800, 450); // screen is 1600 by 900

            // keep camera in level
            if(position.X < 0) {
                position.X = 0;
            }
            if(position.Y < 0) {
                position.Y = 0;
            }
            if(position.X + 1600 > level.Width) {
                position.X = level.Width - 1600;
            }
            if(position.Y + 900 > level.Height) {
                position.Y = level.Height - 900;
            }
        }
    }
}

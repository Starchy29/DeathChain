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
        private const int EDGE_BUFFER = 100;

        private static Vector2 position; // top left

        public static Vector2 Shift { get { return -position; } }

        public static void Start() {
            position = new Vector2(0, 0);
        }

        public static void Update(Level level) {
            // center player in window
            position = Game1.Player.Midpoint - new Vector2(800, 450); // screen is 1600 by 900

            // keep camera in level
            List<Rectangle> edges = level.Edges;
            Rectangle screen = new Rectangle((int)position.X, (int)position.Y, Game1.StartScreenWidth, Game1.StartScreenHeight);
            Vector2 screenMid = screen.Center.ToVector2();

            foreach(Rectangle edge in edges) {
                if( !( // not
                    screen.Top > edge.Bottom - EDGE_BUFFER ||
                    screen.Bottom < edge.Top + EDGE_BUFFER ||
                    screen.Left > edge.Right - EDGE_BUFFER ||
                    screen.Right < edge.Left + EDGE_BUFFER
                )) { // intersecting wall
                    if(screenMid.Y < edge.Top) {
                        // shift up
                        position.Y = edge.Top - Game1.StartScreenHeight + EDGE_BUFFER;
                    }
                    else if(screenMid.Y > edge.Bottom) {
                        // shift down
                        position.Y = edge.Bottom - EDGE_BUFFER;
                    }
                    if(screenMid.X < edge.Left) {
                        // shift left
                        position.X = edge.Left - Game1.StartScreenWidth + EDGE_BUFFER;
                    }
                    else if(screenMid.X > edge.Right) {
                        // shift right
                        position.X = edge.Right - EDGE_BUFFER;
                    }
                }
            }

            // keep camera in level
            /*if(position.X < 0) {
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
            }*/
        }
    }
}

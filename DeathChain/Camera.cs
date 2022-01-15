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

            //Rectangle screen = new Rectangle((int)position.X, (int)position.Y, Game1.StartScreenWidth, Game1.StartScreenHeight);
            //List<Rectangle> areas = level.CameraSpaces;

            // find all sections of the screen that are within bounds
            /*List<Rectangle> coveredAreas = new List<Rectangle>();
            bool topLeft = false; // whether or not these corners are covered
            bool topRight = false;
            bool bottomLeft = false;
            bool bottomRight = false;
            foreach(Rectangle area in areas) {
                if(area.Intersects(screen)) {
                    coveredAreas.Add(Rectangle.Intersect(screen, area));
                    if(area.Contains(new Vector2(screen.Left, screen.Top))) {
                        topLeft = true;
                    }
                    if(area.Contains(new Vector2(screen.Right, screen.Top))) {
                        topRight = true;
                    }
                    if(area.Contains(new Vector2(screen.Left, screen.Bottom))) {
                        bottomLeft = true;
                    }
                    if(area.Contains(new Vector2(screen.Right, screen.Bottom))) {
                        bottomRight = true;
                    }
                }
            }

            bool shifted = false;

            // check for single uncovered corners first: could shift either vertically or horizontally
            if(!topLeft && topRight && bottomLeft && bottomRight) { // top left uncovered
                shifted = true;
                Vector2 bottomRightCorner = new Vector2(screen.Right, screen.Bottom);
                foreach(Rectangle area in coveredAreas) {
                    if(area.Top < bottomRightCorner.Y) {
                        bottomRightCorner.Y = area.Top;
                    }
                    if(area.Left < bottomRightCorner.X) {
                        bottomRightCorner.X = area.Left;
                    }
                }

                // determine which shift direction is closer
                if(bottomRightCorner.X - screen.X < bottomRightCorner.Y - screen.Y) {
                    position.X = bottomRightCorner.X;
                } else {
                    position.Y = bottomRightCorner.Y;
                }
            }
            if(topLeft && !topRight && bottomLeft && bottomRight) { // top right uncovered
                shifted = true;
            }
            if(topLeft && topRight && !bottomLeft && bottomRight) { // bottom left uncovered
                shifted = true;
            }
            if(topLeft && topRight && bottomLeft && !bottomRight) { // bottom right uncovered
                shifted = true;
                Vector2 topLeftCorner = new Vector2(screen.Left, screen.Top);
                foreach(Rectangle area in coveredAreas) {
                    if(area.Bottom > topLeftCorner.Y) {
                        topLeftCorner.Y = area.Bottom;
                    }
                    if(area.Right > topLeftCorner.X) {
                        topLeftCorner.X = area.Right;
                    }
                }

                // determine which shift direction is closer
                if(screen.Right - topLeftCorner.X < screen.Bottom - topLeftCorner.Y) {
                    position.X = topLeftCorner.X - screen.Width;
                } else {
                    position.Y = topLeftCorner.Y - screen.Height;
                }
            }

            if(!shifted) {
                if(!topLeft && !topRight) {
                    // shift down
                    position.Y += screen.Height; // move from bottom up
                    foreach(Rectangle area in coveredAreas) {
                        if(position.Y > area.Top) {
                            position.Y = area.Top;
                        }
                    }
                }
                if(!topLeft && !bottomLeft) {
                    // shift right
                    position.X += screen.Width; // move from right to left
                    foreach(Rectangle area in coveredAreas) {
                        if(position.X > area.Left) {
                            position.X = area.Left;
                        }
                    }
                }
                if(!bottomLeft && !bottomRight) {
                    // shift up
                    position.Y -= screen.Height; // move from top down
                    foreach(Rectangle area in coveredAreas) {
                        if(position.Y + screen.Height < area.Bottom) {
                            position.Y = area.Bottom - screen.Height;
                        }
                    }
                }
                if(!topRight && !bottomRight) {
                    // shift left
                    position.X -= screen.Width; // move from left to right
                    foreach(Rectangle area in coveredAreas) {
                        if(position.X + screen.Width < area.Right) {
                            position.X = area.Right - screen.Width;
                        }
                    }
                }
            }

            // find uncovered space
            /*bool topLeft = false;
            bool topRight = false;
            bool bottomLeft = false;
            bool bottomRight = false;
            foreach(Rectangle area in coveredAreas) {
                if(area.Contains(new Vector2(screen.Left, screen.Top))) {
                    topLeft = true;
                }
                if(area.Contains(new Vector2(screen.Right, screen.Top))) {
                    topRight = true;
                }
                if(area.Contains(new Vector2(screen.Left, screen.Bottom))) {
                    bottomLeft = true;
                }
                if(area.Contains(new Vector2(screen.Right, screen.Bottom))) {
                    bottomRight = true;
                }
            }

            if(!topLeft && !topRight) {

            }
            if(!bottomLeft && !bottomRight) {

            }

            //Rectangle uncovered = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

            // combine the parts of the screen in bounds into one rectangle
            /*if(coveredAreas.Count > 0) {
                Rectangle coveredArea = coveredAreas[0];
                for(int i = 1; i < coveredAreas.Count; i++) {
                    coveredArea = Rectangle.Union(coveredArea, coveredAreas[i]);
                }

                // check if any part of the screen is not covered 
                if(coveredArea.Left > screen.Left) {
                    position.X = coveredArea.Left;
                }
                if(coveredArea.Right < screen.Right) {
                    position.X = coveredArea.Right - screen.Width;
                }
                if(coveredArea.Top > screen.Top) {
                    position.Y = coveredArea.Top;
                }
                if(coveredArea.Bottom < screen.Bottom) {
                    position.Y = coveredArea.Bottom - screen.Height;
                }
            }*/

            /*List<Rectangle> colliders = new List<Rectangle>(); // areas that the screen intersects
            foreach(Rectangle area in areas) {
                if(screen.Intersects(area)) {
                    colliders.Add(area);
                }
            }

            // make sure every side is inside at least one area
            int leftBound = -10000;
            int rightBound = 10000;
            int topBound = -10000;
            int bottomBound = 10000;
            foreach(Rectangle collider in colliders) {
                bool checkTop = true;
                bool checkBottom =true;
                bool checkLeft = true;
                bool checkRight = true;

                foreach(Rectangle other in colliders) { // check if each side collides another collider
                    if(other != collider) {
                        if(other.Contains(new Vector2(collider.Left, collider.Top)) && other.Contains(new Vector2(collider.Right, collider.Top))) {
                            checkTop = false;
                        }
                        if(other.Contains(new Vector2(collider.Left, collider.Bottom)) && other.Contains(new Vector2(collider.Right, collider.Bottom))) {
                            checkBottom = false;
                        }
                        if(other.Contains(new Vector2(collider.Left, collider.Top)) && other.Contains(new Vector2(collider.Left, collider.Bottom))) {
                            checkLeft = false;
                        }
                        if(other.Contains(new Vector2(collider.Right, collider.Top)) && other.Contains(new Vector2(collider.Right, collider.Bottom))) {
                            checkRight = false;
                        }
                    }
                }

                if(checkLeft && collider.Left > leftBound) {
                    leftBound = collider.Left;
                }
                if(checkRight && collider.Right < rightBound) {
                    rightBound = collider.Right;
                }
                if(checkTop && collider.Top > topBound) {
                    topBound = collider.Top;
                }
                if(checkBottom && collider.Bottom < bottomBound) {
                    bottomBound = collider.Bottom;
                }
            }

            if(screen.Left < leftBound) {
                position.X = leftBound;
            }
            else if(screen.Right > rightBound) {
                position.X = rightBound - screen.Width;
            }
            if(screen.Top < topBound) {
                position.Y = topBound;
            }
            else if(screen.Bottom > bottomBound) {
                position.Y = bottomBound - screen.Height;
            }

            /*bool snap = true;
            foreach(Rectangle area in areas) {
                // make sure camera is entirely inside at least one space
                if(area.Contains(screen)) {
                    snap = false;
                    break;
                }
            }

            if(snap) {
                foreach(Rectangle area in areas) {
                    if(area.Intersects(screen)) {
                        if(screen.Left < area.Left) {
                            position.X = area.Left;
                        }
                        else if(screen.Right > area.Right) {
                            position.X = area.Right - screen.Width;
                        }
                        if(screen.Top < area.Top) {
                            position.Y = area.Top;
                        }
                        else if(screen.Bottom > area.Bottom) {
                            position.Y = area.Bottom - screen.Height;
                        }
                    }
                }
            }*/

            /*List<Rectangle> edges = level.Edges;
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
            }*/
        }
    }
}

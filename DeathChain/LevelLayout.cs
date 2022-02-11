using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    // defines the shape of a level and where things can appear
    struct LevelLayout
    {
        public const int DOOR_WIDTH = 200;

        private List<Wall> walls;
        private List<Vector2> spawnSpots; // places where enemies can spawn
        private Vector2 start;

        public List<Wall> Walls { get { return walls; } }
        public List<Vector2> SpawnSpots { get { return spawnSpots; } }
        public Vector2 Start { get { return start; } }
        public int EndY { get { return 0; } }

        public LevelLayout(int region, bool small = false) {
            walls = new List<Wall>();
            start = Vector2.Zero; // placeholder
            spawnSpots = new List<Vector2>();

            // choose edge layout
            switch(Game1.RNG.Next(0, small ? 1 : 2)) {
                case 0: // small room
                    AddEdges(1600, 900);
                    MakeSmallRoom();
                    DefineSpawnSpots(1600, 900);
                    break;
                case 1: // large room
                    AddEdges(2000, 1200);
                    MakeMediumRoom();
                    DefineSpawnSpots(2000, 1200);
                    break;
                case 2: // tall
                    AddEdges(1600, 1600);
                    break;
                case 3: // wide
                    AddEdges(3000, 900, Direction.Right, Direction.Left);
                    break;
                case 4: // reverse wide
                    AddEdges(3000, 900, Direction.Left, Direction.Right);
                    break;
                case 5: // U
                    AddEdges(2000, 2200, Direction.Left, Direction.Left);
                    walls.Add(new Wall(100, 800, 900, 600, false));
                    break;
                case 6: // L
                    AddEdges(2400, 1800, Direction.Right, Direction.Left);
                    walls.Add(new Wall(100, 100, 1100, 900, false));
                    break;
            }
        }

        private void MakeSmallRoom() {
            // 1600 x 900
            switch(Game1.RNG.Next(0, 2)) {
                case 0: // middle pit
                    bool makeLeft = false;
                    bool makeRight = false;
                    if(CoinFlip()) {
                        makeLeft = true;
                        if(CoinFlip()) {
                            makeRight = true;
                        }
                    } else {
                        makeRight = true;
                        if(CoinFlip()) {
                            makeLeft = true;
                        }
                    }

                    const int DIST = 350;
                    if(makeLeft) {
                        walls.Add(new Wall(DIST, DIST, 800 - DIST, 900 - 2 * DIST, CoinFlip()));
                    } else {
                        AddRock(350, 300);
                        AddRock(550, 600);
                    }
                    if(makeRight) {
                        walls.Add(new Wall(800, DIST, 800 - DIST, 900 - 2 * DIST, CoinFlip()));
                    } else {
                        AddRock(1250, 300);
                        AddRock(1050, 600);
                    }
                    break;

                case 1: // dashed box
                    walls.Add(new Wall(800 - 150, 250, 300, 100, CoinFlip())); // top

                    if(CoinFlip()) {
                        walls.Add(new Wall(800 - 150, 550, 300, 100, CoinFlip())); // bottom
                    } else {
                        AddRock(800, 600);
                    }
                    if(CoinFlip()) {
                        walls.Add(new Wall(300, 450 - 150, 100, 300, CoinFlip())); // left
                    } else {
                        AddRock(350, 450);
                    }
                    if(CoinFlip()) {
                        walls.Add(new Wall(1600 - 300 - 100, 450 - 150, 100, 300, CoinFlip())); // right
                    } else {
                        AddRock(1250, 450);
                    }
                    break;
            }
        }

        private void MakeMediumRoom() {
            // 2000 x 1200
            switch(Game1.RNG.Next(0, 2)) {
                case 0: // center dashed line
                    walls.Add(new Wall(1000 - 300, 600 - 50, 600, 100, CoinFlip())); // middle line
                    if(CoinFlip()) {
                        walls.Add(new Wall(100, 600 - 50, 250, 100, CoinFlip())); // left dash of middle line
                    } else {
                        AddRock(350, 600);
                    }
                    if(CoinFlip()) {
                        walls.Add(new Wall(2000 - 100 - 250, 600 - 50, 250, 100, CoinFlip())); // right dash of middle line
                    } else {
                        AddRock(1650, 600);
                    }

                    AddRock(525, 350);
                    AddRock(525, 850);
                    AddRock(1475, 350);
                    AddRock(1475, 850);
                    break;

                case 1: // window
                    Rectangle topLeft = new Rectangle(0, 0, 1000, 600);
                    Rectangle topRight = new Rectangle(1000, 0, 1000, 600);
                    Rectangle bottomLeft = new Rectangle(0, 600, 1000, 600);
                    Rectangle bottomRight = new Rectangle(1000, 600, 1000, 600);
                    topLeft.Inflate(-300, -200);
                    topRight.Inflate(-300, -200);
                    bottomLeft.Inflate(-300, -200);
                    bottomRight.Inflate(-300, -200);
                    topLeft.Offset(100, 100);
                    topRight.Offset(-100, 100);
                    bottomLeft.Offset(100, -100);
                    bottomRight.Offset(-100, -100);

                    if(Game1.RNG.NextDouble() < 0.75) {
                        walls.Add(new Wall(topLeft.X, topLeft.Y, topLeft.Width, topLeft.Height, CoinFlip()));
                    } else {
                        AddRock(topLeft.Center.X, topLeft.Center.Y);
                    }
                    if(Game1.RNG.NextDouble() < 0.75) {
                        walls.Add(new Wall(topRight.X, topRight.Y, topRight.Width, topRight.Height, CoinFlip()));
                    } else {
                        AddRock(topRight.Center.X, topRight.Center.Y);
                    }
                    if(Game1.RNG.NextDouble() < 0.75) {
                        walls.Add(new Wall(bottomLeft.X, bottomLeft.Y, bottomLeft.Width, bottomLeft.Height, CoinFlip()));
                    } else {
                        AddRock(bottomLeft.Center.X, bottomLeft.Center.Y);
                    }
                    if(Game1.RNG.NextDouble() < 0.75) {
                        walls.Add(new Wall(bottomRight.X, bottomRight.Y, bottomRight.Width, bottomRight.Height, CoinFlip()));
                    } else {
                        AddRock(bottomRight.Center.X, bottomRight.Center.Y);
                    }
                    break;
            }
        }

        private void AddEdges(int width, int height, Direction topDoor = Direction.None, Direction bottomDoor = Direction.None) {
            const int W = 100; // wall width
            const int SIDE_DIST = 400; // distance doors are from the wall

            walls.Add(new Wall(0, 0, W, height, false)); // left
            walls.Add(new Wall(width - W, 0, W, height, false)); // right
            
            if(topDoor == Direction.Left) {
                int x = W;
                walls.Add(new Wall(x, 0, SIDE_DIST, W, false));
                x += SIDE_DIST;
                walls.Insert(0, new Wall(x, 0, DOOR_WIDTH, W, false));
                x += DOOR_WIDTH;
                walls.Add(new Wall(x, 0, width - W - x, W, false));
            }
            else if(topDoor == Direction.Right) {
                int x = width - W - SIDE_DIST;
                walls.Add(new Wall(x, 0, SIDE_DIST, W, false));
                x -= DOOR_WIDTH;
                walls.Insert(0, new Wall(x, 0, DOOR_WIDTH, W, false));
                walls.Add(new Wall(W, 0, x - W, W, false));
            }
            else { // middle
                int doorBorder = (width - 2 * W - DOOR_WIDTH) / 2;
                walls.Add(new Wall(W, 0, doorBorder, W, false)); // top left
                walls.Add(new Wall(width - W - doorBorder, 0, doorBorder, W, false)); // top right
                walls.Insert(0, new Wall(width / 2 - DOOR_WIDTH / 2, 0, DOOR_WIDTH, W, false));
            }

            if(bottomDoor == Direction.Left) {
                int x = 100;
                walls.Add(new Wall(x, height - W, SIDE_DIST, W, false));
                x += SIDE_DIST;
                // add door
                x += DOOR_WIDTH;
                walls.Add(new Wall(x, height - W, width - W - x, W, false));

                start = new Vector2(W + SIDE_DIST + DOOR_WIDTH / 2, height);
            }
            else if(bottomDoor == Direction.Right) {
                int x = width - W - SIDE_DIST;
                walls.Add(new Wall(x, height - W, SIDE_DIST, W, false));
                x -= DOOR_WIDTH;
                // add door
                walls.Add(new Wall(W, height - W, x - W, W, false));

                start = new Vector2(width - W - SIDE_DIST - DOOR_WIDTH / 2, height);
            }
            else { // middle
                int doorBorder = (width - 2 * W - DOOR_WIDTH) / 2;
                walls.Add(new Wall(W, height - W, doorBorder, W, false)); // bottom left
                walls.Add(new Wall(width - W - doorBorder, height - W, doorBorder, W, false)); // bottom right

                start = new Vector2(width / 2, height);
            }
        }

        // must be called after the walls are placed
        private void DefineSpawnSpots(int width, int height) {
            int tileWidth = 150;

            // cut the region into tiles
            for(int x = 0; x < width; x += tileWidth) {
                for(int y = 0; y < height - 300; y += tileWidth) { // don't place at bottom
                    Rectangle tile = new Rectangle(x, y, tileWidth, tileWidth);
                    
                    // check if tile collides with any walls / pits
                    bool clear = true;
                    foreach(Wall wall in walls) {
                        if(wall.Hitbox.Intersects(tile)) {
                            clear = false;
                            break;
                        }
                    }

                    if(clear) {
                        spawnSpots.Add(new Vector2(tile.Center.X, tile.Center.Y));
                    }
                }
            }
        }

        private bool CoinFlip() {
            return Game1.RNG.NextDouble() < 0.5;
        }

        private void AddRock(int x, int y) {
            int width = 70;
            if(CoinFlip()) {
                walls.Add(new Wall(x - width / 2, y - width / 2, width, width, false));
            }
        }
    }
}

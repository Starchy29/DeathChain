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
            Random rng = new Random();

            walls = new List<Wall>();
            start = Vector2.Zero; // placeholder
            spawnSpots = new List<Vector2>();

            // choose edge layout
            switch(rng.Next(0, small ? 1 : 2)) {
                case 0: // small room
                    AddEdges(1600, 900);
                    MakeSmallRoom(rng);
                    break;
                case 1: // large room
                    AddEdges(2000, 1200);
                    MakeMediumRoom(rng);
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

        private void MakeSmallRoom(Random rng) {
            // 1600 x 900
            switch(rng.Next(0, 2)) {
                case 0: // middle pit
                    bool makeLeft = false;
                    bool makeRight = false;
                    if(CoinFlip(rng)) {
                        makeLeft = true;
                        if(CoinFlip(rng)) {
                            makeRight = true;
                        }
                    } else {
                        makeRight = true;
                        if(CoinFlip(rng)) {
                            makeLeft = true;
                        }
                    }

                    const int DIST = 350;
                    if(makeLeft) {
                        walls.Add(new Wall(DIST, DIST, 800 - DIST, 900 - 2 * DIST, CoinFlip(rng)));
                    } else {
                        spawnSpots.Add(new Vector2(700, 500)); // lower left
                        spawnSpots.Add(new Vector2(600, 400)); // upper left
                        AddRock(350, 300, rng);
                        AddRock(550, 600,rng);
                    }
                    if(makeRight) {
                        walls.Add(new Wall(800, DIST, 800 - DIST, 900 - 2 * DIST, CoinFlip(rng)));
                    } else {
                        spawnSpots.Add(new Vector2(900, 500)); // lower right
                        spawnSpots.Add(new Vector2(1000, 400)); // upper right
                        AddRock(1250, 300, rng);
                        AddRock(1050, 600, rng);
                    }

                    // corners
                    spawnSpots.Add(new Vector2(100 + 150, 100 + 150)); // top left
                    spawnSpots.Add(new Vector2(1600 - 100 - 150, 100 + 150)); // top right
                    spawnSpots.Add(new Vector2(100 + 150, 900 - 100 - 150)); // bottom left
                    spawnSpots.Add(new Vector2(1600 - 100 - 150, 900 - 100 - 150)); // bottom right

                    // sides
                    spawnSpots.Add(new Vector2(100 + 75, 450)); // left
                    spawnSpots.Add(new Vector2(1600 - 100 - 75, 450)); // right
                    break;

                case 1: // rocks
                    walls.Add(new Wall(800 - 150, 250, 300, 100, CoinFlip(rng))); // top

                    if(CoinFlip(rng)) {
                        walls.Add(new Wall(800 - 150, 550, 300, 100, CoinFlip(rng))); // bottom
                    }
                    if(CoinFlip(rng)) {
                        walls.Add(new Wall(300, 450 - 150, 100, 300, CoinFlip(rng))); // left
                    }
                    if(CoinFlip(rng)) {
                        walls.Add(new Wall(1600 - 300 - 100, 450 - 150, 100, 300, CoinFlip(rng))); // right
                    }

                    spawnSpots.Add(new Vector2(100 + 75, 450)); // left
                    spawnSpots.Add(new Vector2(1600 - 100 - 75, 450)); // right

                    spawnSpots.Add(new Vector2(1600 - 300 - 50, 200)); // top right
                    spawnSpots.Add(new Vector2(1600 - 300 - 50, 900 - 200)); // bottom right
                    spawnSpots.Add(new Vector2(300 + 50, 200)); // top left
                    spawnSpots.Add(new Vector2(300 + 50, 900 - 200)); // bottom left

                    spawnSpots.Add(new Vector2(800, 200)); // middle
                    spawnSpots.Add(new Vector2(800, 450)); // top middle
                    break;
            }
        }

        private void MakeMediumRoom(Random rng) {
            // 2000 x 1200
            switch(0) {
                case 0: // center dashed line
                    walls.Add(new Wall(1000 - 300, 600 - 50, 600, 100, CoinFlip(rng))); // middle line
                    if(CoinFlip(rng)) {
                        walls.Add(new Wall(100, 600 - 50, 250, 100, CoinFlip(rng))); // left dash of middle line
                    }
                    if(CoinFlip(rng)) {
                        walls.Add(new Wall(2000 - 100 - 250, 600 - 50, 250, 100, CoinFlip(rng))); // right dash of middle line
                    }

                    spawnSpots.Add(new Vector2(1000, 300)); // center top

                    // left column (350 - 700)
                    spawnSpots.Add(new Vector2(525 + 150, 300)); // top
                    spawnSpots.Add(new Vector2(525, 600)); // middle
                    spawnSpots.Add(new Vector2(525 + 150, 900)); // bottom

                    // right column (1300 - 1650)
                    spawnSpots.Add(new Vector2(1475 - 150, 300)); // top
                    spawnSpots.Add(new Vector2(1475, 600)); // middle
                    spawnSpots.Add(new Vector2(1475 - 150, 900)); // bottom

                    // corners
                    spawnSpots.Add(new Vector2(100 + 150, 100 + 150)); // top left
                    spawnSpots.Add(new Vector2(2000 - 100 - 150, 100 + 150)); // top right
                    spawnSpots.Add(new Vector2(100 + 150, 1200 - 100 - 150)); // bottom left
                    spawnSpots.Add(new Vector2(2000 - 100 - 150, 1200 - 100 - 150)); // bottom right
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

        private bool CoinFlip(Random rng) {
            return rng.NextDouble() < 0.5;
        }

        private void AddRock(int x, int y, Random rng) {
            int width = 50;
            if(CoinFlip(rng)) {
                walls.Add(new Wall(x - width / 2, y - width / 2, width, width, false));
            }
        }
    }
}

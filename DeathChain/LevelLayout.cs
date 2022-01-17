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

        public LevelLayout(int region) {
            Random rng = new Random();

            walls = new List<Wall>();
            start = Vector2.Zero; // placeholder
            spawnSpots = new List<Vector2>();

            // choose edge layout
            switch(1 + 0 * rng.Next(0, 7)) {
                case 0: // small room
                    AddEdges(1600, 900);
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
    }
}

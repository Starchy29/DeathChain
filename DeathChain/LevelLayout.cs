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
        private List<Wall> edges;
        private List<Rectangle> midWalls; // can be pit or wall, 50/50 each time
        private List<Vector2> spawnSpots; // places where enemies can spawn

        public List<Wall> Edges { get { return edges; } }
        public List<Rectangle> Obstacles { get { return midWalls; } }
        public List<Vector2> SpawnSpots { get { return spawnSpots; } }
        public LevelLayout(int width, int height, List<Wall> edges, List<Rectangle> midWalls, List<Vector2> spawnSpots) {
            this.edges = edges;
            this.midWalls = midWalls;
            this.spawnSpots = spawnSpots;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    public class Level
    {
        private delegate bool EdgeCheck(Rectangle tester, Rectangle outermost);

        private static Random rng = new Random(); 

        private List<Particle> particles;
        private List<Projectile> projectiles;
        private List<Enemy> enemies;
        private List<Wall> walls;

        private List<Rectangle> edges; // walls that define the edges of the level
        public List<Rectangle> Edges { get { return edges; } } // used by the camera

        public List<Particle> Particles { get { return particles; } }
        public List<Projectile> Projectiles { get { return projectiles; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public List<Wall> Walls { get { return walls; } }

        public Level() {
            // create a sample level
            particles = new List<Particle>();
            projectiles = new List<Projectile>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            //walls.Add(new Wall(100, 0, 1400, 100, false));
            //walls.Add(new Wall(0, 0, 100, 900, false));
            walls.Add(new Wall(1500, 0, 500, 900, false));
            walls.Add(new Wall(100, 800, 1400, 100, false));

            walls.Add(new Wall(0, -1000, 100, 1900, false));
            walls.Add(new Wall(0, -1100, 2100, 100, false));

            walls.Add(new Wall(1000, 400, 150, 150, false));
            walls.Add(new Wall(400, 400, 150, 150, true));

            /*enemies.Add(new Zombie(1300, 300));
            enemies.Add(new Zombie(1300, 500));
            enemies.Add(new Zombie(1300, 700));

            //enemies.Add(new Mushroom(300, 450));
            enemies.Add(new Mushroom(1300, 450));

            enemies.Add(new Slime(300, 450));*/

            DefineEdges();
        }

        // create a random level with a certain difficulty
        public Level(int difficulty) {
            particles = new List<Particle>();
            projectiles = new List<Projectile>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            // define possible enemies and their power level
            List<EnemyTypes>[] enemyTypes = new List<EnemyTypes>[3];
            enemyTypes[0] = new List<EnemyTypes>();
            enemyTypes[1] = new List<EnemyTypes>();
            enemyTypes[2] = new List<EnemyTypes>();

            enemyTypes[0].Add(EnemyTypes.Zombie);
            enemyTypes[0].Add(EnemyTypes.Mushroom);
            enemyTypes[1].Add(EnemyTypes.Slime);

            // choose a level shape
            LevelLayout layout = ChooseLevelLayout();
            //width = layout.Width

            // choose a floorplan
            List<Rectangle> obstacles = layout.Obstacles;
            foreach(Rectangle obstacle in obstacles) {
                walls.Add(new Wall(obstacle.X, obstacle.Y, obstacle.Width, obstacle.Height, rng.NextDouble() < 0.5));
            }

            // shuffle spawn spots
            List<Vector2> spawnSpots = layout.SpawnSpots;
            
            // add enemies
            int enemyMin = 3;
            if(difficulty < 3) {
                enemyMin = difficulty;
            }

            DefineEdges();
        }

        public void Update(float deltaTime) {
            foreach(Enemy enemy in enemies) {
                enemy.Update(this, deltaTime);
            }
            foreach(Projectile projectile in projectiles) {
                projectile.Update(this, deltaTime);
            }
            foreach(Particle particle in particles) {
                particle.Update(deltaTime);
            }

            // clear dead enemies and projectiles, and completed particles
            for(int i = 0; i < enemies.Count; i++) {
                if(!enemies[i].IsActive) {
                    enemies.RemoveAt(i);
                    i--;
                }
            }

            for(int i = 0; i < projectiles.Count; i++) {
                if(!projectiles[i].IsActive) {
                    projectiles.RemoveAt(i);
                    i--;
                }
            }

            for(int i = 0; i < particles.Count; i++) {
                if(particles[i].Done) {
                    particles.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Draw(SpriteBatch sb) {
            // draw background
            sb.Draw(Graphics.Pixel, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.DarkGreen * 0.5f);

            // draw level
            foreach (Wall wall in walls) { // allow entites to overlap with walls
                wall.Draw(sb);
            }

            foreach(Projectile projectile in projectiles) { // projectiles under enemies looks better
                projectile.Draw(sb);
            }

            foreach(Enemy enemy in enemies) { // enemies before walls so if clipping happens, it's hidden
                enemy.Draw(sb);
            }

            Game1.Player.Draw(sb);

            foreach(Particle particle in particles) { // particles are typically small enough to fit on top of entities
                particle.Draw(sb);
            }

            Game1.Player.DrawUI(sb);
        }

        private LevelLayout ChooseLevelLayout() {
            return new LevelLayout();
        }

        private void DefineEdges() {
            edges = new List<Rectangle>();

            edges.AddRange(FindEdges( // leftmost
                (Rectangle tester, Rectangle outermost) => { return tester.Left == outermost.Left; },
                (Rectangle tester, Rectangle outermost) => { return tester.Left < outermost.Left; }
            ));

            List<Rectangle> newEdges = FindEdges( // rightmost
                (Rectangle tester, Rectangle outermost) => { return tester.Right == outermost.Right; },
                (Rectangle tester, Rectangle outermost) => { return tester.Right > outermost.Right; }
            );
            foreach(Rectangle newEdge in newEdges) {
                if(!edges.Contains(newEdge)) {
                    edges.Add(newEdge);
                }
            }

            newEdges = FindEdges( // top
                (Rectangle tester, Rectangle outermost) => { return tester.Top == outermost.Top; },
                (Rectangle tester, Rectangle outermost) => { return tester.Top < outermost.Top; }
            );
            foreach(Rectangle newEdge in newEdges) {
                if(!edges.Contains(newEdge)) {
                    edges.Add(newEdge);
                }
            }

            newEdges = FindEdges( // bottom
                (Rectangle tester, Rectangle outermost) => { return tester.Bottom == outermost.Bottom; },
                (Rectangle tester, Rectangle outermost) => { return tester.Bottom > outermost.Bottom; }
            );
            foreach(Rectangle newEdge in newEdges) {
                if(!edges.Contains(newEdge)) {
                    edges.Add(newEdge);
                }
            }

            List<Rectangle> FindEdges(EdgeCheck linesUp, EdgeCheck past) {
                List<Rectangle> edges = new List<Rectangle>();
                foreach(Wall wall in walls) {
                    Rectangle zone = wall.Hitbox;

                    if(edges.Count <= 0) {
                        edges.Add(zone);
                    }
                    else if(linesUp(zone, edges[0])) {
                        // use all zones that share the outer edge
                        edges.Add(zone);
                    }
                    else if(past(zone, edges[0])) {
                        edges.Clear();
                        edges.Add(zone);
                    }
                }
                return edges;
            }
        }
    }
}

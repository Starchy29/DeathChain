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
        private int width;
        private int height;

        private List<Particle> particles;
        private List<Projectile> projectiles;
        private List<Enemy> enemies;
        private List<Wall> walls;

        public List<Particle> Particles { get { return particles; } }
        public List<Projectile> Projectiles { get { return projectiles; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public List<Wall> Walls { get { return walls; } }
        public int Width { get {return width; } }
        public int Height { get { return height; } }

        public Level() {
            // create a sample level
            width = 1600;
            height = 900;

            particles = new List<Particle>();
            projectiles = new List<Projectile>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            walls.Add(new Wall(0, 0, 1600, 100, false));
            walls.Add(new Wall(0, 100, 100, 900, false));
            walls.Add(new Wall(1500, 0, 100, 900, false));
            walls.Add(new Wall(0, 800, 1600, 100, false));

            walls.Add(new Wall(1000, 400, 150, 150, false));
            walls.Add(new Wall(400, 400, 150, 150, true));

            enemies.Add(new Zombie(1300, 300));
            enemies.Add(new Zombie(1300, 500));
            enemies.Add(new Zombie(1300, 700));

            //enemies.Add(new Mushroom(300, 450));
            enemies.Add(new Mushroom(1300, 450));

            enemies.Add(new Slime(300, 450));
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

            // draw level
            foreach(Wall wall in walls) { // allow entites to overlap with walls
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
    }
}

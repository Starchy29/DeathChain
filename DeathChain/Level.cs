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

        private List<Entity> projectiles;
        private List<Enemy> enemies;
        private List<Wall> walls;

        public List<Entity> Projectiles { get { return projectiles; } }
        public List<Enemy> Enemies { get { return enemies; } }
        public List<Wall> Walls { get { return walls; } }

        public Level() {
            // create a sample level
            projectiles = new List<Entity>();
            enemies = new List<Enemy>();
            walls = new List<Wall>();

            walls.Add(new Wall(0, 0, 1600, 100, false));
            walls.Add(new Wall(0, 100, 100, 900, false));
            walls.Add(new Wall(1500, 0, 100, 900, false));
            walls.Add(new Wall(0, 800, 1600, 100, false));

            walls.Add(new Wall(800, 400, 150, 150, false));
        }

        public void Update(float deltaTime) {
            foreach(Enemy enemy in enemies) {
                enemy.Update(this, deltaTime);
            }
            foreach(Entity projectile in projectiles) {
                projectile.Update(this, deltaTime);
            }
        }

        public void Draw(SpriteBatch sb) {
            // draw background

            // daw entities
            foreach(Wall wall in walls) {
                wall.Draw(sb);
            }
            foreach(Enemy enemy in enemies) {
                enemy.Draw(sb);
            }
            foreach(Entity projectile in projectiles) {
                projectile.Draw(sb);
            }
        }
    }
}

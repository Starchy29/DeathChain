using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public abstract class Enemy : Entity
    {
        protected int health;
        private int maxHealth; // tells the player how much health to have when possessing this
        protected bool alive;
        protected float timer;
        private EnemyTypes type;

        public EnemyTypes Type { get { return type; } }
        public bool Alive { get { return alive; } }
        public int MaxHealth { get { return maxHealth; } }
        public Rectangle DrawRect { get { return drawBox; } }

        public Enemy(EnemyTypes type, int x, int y, int width, int height, int health) : base(x, y, width, height, Graphics.Pixel) {
            alive = true;
            this.health = health;
            this.type = type;
            maxHealth = health;
        }

        public sealed override void Update(Level level, float deltaTime) {
            if(alive) {
                AliveUpdate(level, deltaTime);

                if(Hitbox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamage(1);
                }
            } else {
                tint = Color.White;
                if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= Player.SELECT_DIST) {
                    tint = Color.LightBlue;
                }
            }
        }

        protected abstract void AliveUpdate(Level level, float deltaTime);

        public virtual void TakeDamage(int damage) {
            health -= damage;
            if(health <= 0) {
                // die
                alive = false;
            }
        }

        // moves toward player
        protected void Seek() {

        }

        // moves away from other enemies
        protected void Separate(Level level, float deltaTime) {
            foreach(Enemy enemy in level.Enemies) {
                if(enemy != this && Vector2.Distance(Midpoint, enemy.Midpoint) <= 100) {
                    Vector2 moveAway = Midpoint - enemy.Midpoint;
                    velocity += moveAway * 10 * deltaTime;
                }
            }
        }
    }
}

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
        protected bool alive;
        protected Vector2 velocity;

        public Enemy(int x, int y, int width, int height, int health) : base(x, y, width, height, Graphics.Pixel) {
            alive = true;
            this.health = health;
        }

        public sealed override void Update(Level level, float deltaTime) {
            if(alive) {
                AliveUpdate(level, deltaTime);

                if(Hitbox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamge(1);
                }
            } else {
                tint = Color.White;
                if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= 80) {
                    tint = Color.LightBlue;
                }
            }
        }

        protected abstract void AliveUpdate(Level level, float deltaTime);

        public void TakeDamage(int damage) {
            health -= damage;
            if(health <= 0) {
                // die
                alive = false;
            }
        }

        public void Push(Vector2 force) {
            velocity += force;
        }
    }
}

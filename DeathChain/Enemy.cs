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
        protected bool alive; // alive determines if the player can possess this, isActive determines if it should be deleted
        protected float timer;
        private float enemyTimer; // alive: red flash when hit, dead: despawn timer

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
            if(enemyTimer > 0) {
                enemyTimer -= deltaTime;
            }

            if(alive) {
                AliveUpdate(level, deltaTime);

                if(Hitbox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamage(1);
                }
            } 
            else if(enemyTimer <= 0) {
                // decay body after some time
                IsActive = false;
            }
        }

        public override void Draw(SpriteBatch sb) {
            tint = Color.White;
            if(alive && enemyTimer > 0) {
                tint = Color.Red;
            }
            else if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= Player.SELECT_DIST) {
                tint = Color.LightBlue;
            }
            else if(!alive) {
                // temporary
                tint = Color.Black;
            }
            base.Draw(sb);
        }

        protected abstract void AliveUpdate(Level level, float deltaTime);

        public virtual void TakeDamage(int damage) {
            health -= damage;
            enemyTimer = 0.1f; // red flash duration
            if(health <= 0) {
                // die
                alive = false;
                enemyTimer = 5; // time before body despawns;
            }
        }

        // moves away from other enemies
        protected void Separate(Level level, float deltaTime) {
            foreach(Enemy enemy in level.Enemies) {
                if(enemy != this && enemy.alive && Vector2.Distance(Midpoint, enemy.Midpoint) <= 100) {
                    Vector2 moveAway = Midpoint - enemy.Midpoint;
                    velocity += moveAway * 10 * deltaTime;
                }
            }
        }
    }
}

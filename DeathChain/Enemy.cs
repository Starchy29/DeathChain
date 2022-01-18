﻿using System;
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
        protected const int ACCEL = 2000;

        protected int health;
        private int maxHealth; // tells the player how much health to have when possessing this
        protected bool alive; // alive determines if the player can possess this, isActive determines if it should be deleted
        protected float timer;
        private float enemyTimer; // alive: red flash when hit, dead: despawn timer
        protected Vector2 direction; // the direction this moves towards, determined by sub classes
        protected int maxSpeed;

        private EnemyTypes type;

        public EnemyTypes Type { get { return type; } }
        public bool Alive { get { return alive; } }
        public int MaxHealth { get { return maxHealth; } }
        public Rectangle DrawRect { get { return drawBox; } }

        public Enemy(EnemyTypes type, Vector2 midpoint, int width, int height, int health, int maxSpeed) : base(midpoint, width, height, Graphics.Pixel) {
            alive = true;
            this.health = health;
            this.type = type;
            this.maxSpeed = maxSpeed;
            maxHealth = health;
        }

        public sealed override void Update(Level level, float deltaTime) {
            if(enemyTimer > 0) {
                enemyTimer -= deltaTime;
            }

            if(alive) {
                AliveUpdate(level, deltaTime);

                // move in target direction, cap max speed
                if(direction != Vector2.Zero) {
                    direction.Normalize();
                }
                velocity += direction * ACCEL * deltaTime;
                if(velocity.Length() > maxSpeed) {
                    velocity.Normalize();
                    velocity *= maxSpeed;
                }
                position += velocity * deltaTime;

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
            else if(!alive && !Game1.Player.Possessing && Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= Player.SELECT_DIST) {
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

        protected void ApplyFriction(float deltaTime, float amount = 1000) {
            Vector2 direction = -velocity;
            if(direction != Vector2.Zero) {
                direction.Normalize();
                velocity += direction * deltaTime * amount;
                if(Vector2.Dot(direction, velocity) > 0) {
                    // started moving backwards: stop instead
                    velocity = Vector2.Zero;
                }
            }
        }
    }
}

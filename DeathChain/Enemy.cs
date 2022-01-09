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
        protected int health;
        private int maxHealth; // tells the player how much health to have when possessing this
        protected bool alive; // alive determines if the player can possess this, isActive determines if it should be deleted
        protected float timer;
        private float damageTime; // enemy appears red when getting hit

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
                if(damageTime > 0) {
                    damageTime -= deltaTime;
                }

                AliveUpdate(level, deltaTime);

                if(Hitbox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamage(1);
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            tint = Color.White;
            if(alive && damageTime > 0) {
                tint = Color.Red;
            }
            else if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= Player.SELECT_DIST) {
                tint = Color.LightBlue;
            }
            base.Draw(sb);
        }

        protected abstract void AliveUpdate(Level level, float deltaTime);

        public virtual void TakeDamage(int damage) {
            health -= damage;
            damageTime = 0.1f;
            if(health <= 0) {
                // die
                alive = false;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Projectile : Entity
    {
        private bool fromPlayer;
        private Particle burst;
        private Particle trail;
        private float trailTimer;
        private readonly float trailFreq;
        public bool FromPlayer { get { return fromPlayer; } }

        public Projectile(Vector2 midpoint, Vector2 velocity, bool fromPlayer, int length, Texture2D sprite, Particle burst = null, Particle trail = null)
            : base((int)midpoint.X - length / 2, (int)midpoint.Y - length / 2, length, length, sprite) {
            this.velocity = velocity;
            this.fromPlayer = fromPlayer;
            this.burst = burst;
            this.trail = trail;
            if(trail != null) {
                // calculate trail frequency based on velocity and size
                float speed = velocity.Length();
                trailFreq = length / speed;
            }
        }

        public override void Update(Level level, float deltaTime) {
            position += velocity * deltaTime;

            List<Direction> collisions = CheckWallCollision(level, false);
            if(collisions.Count > 0) {
                IsActive = false;
                if(burst != null) {
                    level.Particles.Add(new Particle(burst, Midpoint));
                }
            } else {
                // leave a trail effect (not on the frame this hits a wall)
                if(trail != null) {
                    trailTimer += deltaTime;
                    if(trailTimer >= trailFreq) {
                        trailTimer = 0;
                        Vector2 direction = -velocity;
                        if(direction.Length() > 0) {
                            direction.Normalize();
                        }
                        level.Particles.Add(new Particle(trail, Midpoint + direction * width));
                    }
                }
            }

            // check if hit a target
            Vector2 knockback = velocity;
            knockback.Normalize();
            knockback *= 500;
            if(fromPlayer) {
                foreach(Enemy enemy in level.Enemies) {
                    if(enemy.Alive && Collides(enemy)) {
                        enemy.TakeDamage(1);
                        enemy.Push(knockback);
                        IsActive = false;
                    }
                }
            } else {
                if(Collides(Game1.Player)) {
                    Game1.Player.TakeDamage(1);
                    Game1.Player.Push(knockback);
                    IsActive = false;
                }
            }
        }
    }
}

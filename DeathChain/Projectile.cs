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
        protected bool fromPlayer;
        private Particle burst;
        private Particle trail;
        private float trailTimer;
        private readonly float trailFreq;
        protected float distanceLeft;
        public bool FromPlayer { get { return fromPlayer; } }

        public Projectile(Vector2 midpoint, Vector2 velocity, float range, int length, bool fromPlayer, Texture2D[] sprites, Particle burst = null, Particle trail = null)
            : base(midpoint, length, length) {
            this.velocity = velocity;
            this.fromPlayer = fromPlayer;
            this.burst = burst;
            this.trail = trail;
            if(trail != null) {
                // calculate trail frequency based on velocity and size
                float speed = velocity.Length();
                trailFreq = length / speed;
            }
            this.distanceLeft = range;

            currentAnimation = new Animation(sprites, AnimationType.Loop, 0.2f);
        }

        // create a projectile blueprint
        public Projectile(float speed, float range, int length, Texture2D[] sprites, Particle burst = null, Particle trail = null) :
            this(Vector2.Zero, new Vector2(speed, 0), range, length, false, sprites, burst, trail) { } // the literal values here will be set to the correct value when copied, so they can be anything

        // copy a projectile from an existing one
        public Projectile(Projectile other, Vector2 midpoint, Vector2 aim, bool fromPlayer) :
            base(midpoint, other.Width, other.Height, other.sprite)
        {
            if(aim.Length() > 0) {
                aim.Normalize();
            }
            velocity = aim * other.velocity.Length();
            this.fromPlayer = fromPlayer;
            this.trail = other.trail;
            this.burst = other.burst;
            this.trailTimer = other.trailTimer;
            this.trailFreq = other.trailFreq;
            this.distanceLeft = other.distanceLeft;
            this.currentAnimation = new Animation(other.currentAnimation);
        }

        public override void Update(Level level, float deltaTime) {
            currentAnimation.Update(deltaTime);

            Vector2 displacement = velocity * deltaTime;
            position += displacement;
            distanceLeft -= displacement.Length();
            if(distanceLeft <= 0) {
                IsActive = false;
            }

            Vector2 lastVelocity = velocity;
            List<Direction> collisions = CheckWallCollision(level, false);
            if(collisions.Count > 0) {
                OnWallHit(collisions, lastVelocity);
            } else {
                // leave a trail effect (not on the frame this hits a wall)
                if(trail != null) {
                    trailTimer += deltaTime;
                    if(trailTimer >= trailFreq) {
                        trailTimer = 0;
                        Vector2 direction = -velocity;
                        float rotation = 0f;
                        if(direction.Length() > 0) {
                            direction.Normalize();
                            rotation = (float)Math.Atan2(direction.Y, direction.X);
                        }
                        level.Particles.Add(new Particle(trail, Midpoint + direction * width, rotation));
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
                        enemy.TakeDamage(level);
                        enemy.Push(knockback);
                        IsActive = false;
                    }
                }
            } else {
                if(Collides(Game1.Player)) {
                    Game1.Player.TakeDamage(level);
                    Game1.Player.Push(knockback);
                    IsActive = false;
                }
            }

            if(!IsActive && burst != null) {
                level.Particles.Add(new Particle(burst, Midpoint));
            }
        }

        public override void Draw(SpriteBatch sb) {
            float rotation = 0f;
            if(velocity != Vector2.Zero) {
                rotation = Game1.GetVectorAngle(velocity);
            }

            if(currentAnimation.CurrentSprite != null) {
                Graphics.RotateDraw(sb, currentAnimation.CurrentSprite, DrawBox, tint, rotation);
            }
        }

        // for sub classes
        protected virtual void OnWallHit(List<Direction> collisions, Vector2 hitVelocity) {
            IsActive = false;
        }
    }
}

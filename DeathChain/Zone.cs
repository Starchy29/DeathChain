using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace DeathChain
{
    // a circle that sits on the battlefield
    class Zone : Entity
    {
        private float duration; // includes startup time
        private float startup; // time when zone is visible before interacting
        private Circle hitZone;
        private float timer;
        private Particle dissipation;
        protected bool fromPlayer;

        public Zone(Vector2 midpoint, bool fromPlayer, int radius, float duration, float startup, Texture2D[] sprites, bool stillAnim, Particle dissipation) : base(midpoint, 2 * radius, 2 * radius) {
            // animation either occupies the whole time or just the startup
            if(stillAnim) {
                currentAnimation = new Animation(sprites, AnimationType.Hold, startup / sprites.Length);
            } else {
                currentAnimation = new Animation(sprites, AnimationType.Hold, duration / sprites.Length);
            }
            
            this.duration = duration;
            hitZone = new Circle(midpoint, radius - 10); // shrink hitzone a bit
            this.startup = startup;
            this.dissipation = dissipation;
            this.fromPlayer = fromPlayer;
        }

        // copy from an existing zone
        public Zone(Zone other, Vector2 midpoint, bool fromPlayer) : base(midpoint, other.width, other.height) {
            currentAnimation = other.currentAnimation;
            this.fromPlayer = fromPlayer;

            this.duration = other.duration;
            hitZone = new Circle(midpoint, other.hitZone.Radius); // shrink hitzone a bit
            this.startup = other.startup;
            this.dissipation = other.dissipation;
            
        }

        public override void Update(Level level, float deltaTime) {
            currentAnimation.Update(deltaTime);

            timer += deltaTime;
            if(timer >= startup) {
                if(fromPlayer) {
                    foreach(Enemy enemy in level.Enemies) {
                        if(enemy.Alive && enemy.HitCircle.Intersects(hitZone)) {
                            OnHit(enemy);
                            break;
                        }
                    }
                }
                else if(Game1.Player.HitCircle.Intersects(hitZone)) {
                    OnHit();
                }
            }

            if(timer >= duration || !IsActive) { // if time is up or ending early
                this.IsActive = false;
                if(dissipation != null) {   
                    level.Particles.Add(new Particle(dissipation, Midpoint));
                }
            }
        }

        protected virtual void OnHit(Enemy enemy = null) {
            // default: deal damage
            if(fromPlayer) {
                enemy.TakeDamage();
            } else {
                Game1.Player.TakeDamage();
            }
            this.IsActive = false;
        }
    }
}

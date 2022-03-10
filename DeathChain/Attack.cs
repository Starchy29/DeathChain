using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    // a melee attack that can swipe around a source
    class Attack : Entity
    {
        private Entity user;
        private float duration;
        private float rotateSpeed;
        private float timePassed;
        private Vector2 startAim;
        List<Entity> hitEntities;
        private float drawRotation;
        private SpriteEffects flips; // flip sprites when attacking the other rotation

        public Attack(Entity user, int diameter, Vector2 startAim, float rotation, float duration, Texture2D[] sprites, bool reversed = false) : base(user.Midpoint + (user.Width + diameter / 2f) / 2f * startAim, diameter, diameter) {
            if(duration <= 0) {
                throw new ArgumentException("Attempted to create an attack with non-positive duration");
            }

            hitEntities = new List<Entity>();

            this.user = user;
            this.duration = duration;
            this.startAim = startAim;

            timePassed = 0;
            rotateSpeed = rotation / duration;

            if(startAim != Vector2.Zero) {
                this.startAim.Normalize();
            }

            if(sprites != null) {
                currentAnimation = new Animation(sprites, AnimationType.Hold, duration / sprites.Length); // animation evenly occupies entire attack
                //sprite = sprites[0];
            } else {
                sprite = Graphics.Pixel;
            }

            drawRotation = (float)Math.Atan2(startAim.Y, startAim.X) + rotation / 2f;
            flips = SpriteEffects.None;
            if(reversed) {
                flips = SpriteEffects.FlipVertically;
            }
        }

        public override void Update(Level level, float deltaTime) {
            currentAnimation.Update(deltaTime);

            // update duration timer
            timePassed += deltaTime;
            if(timePassed >= duration) {
                this.IsActive = false; // end attack. Each entity should check if their own attack has ended
            }

            // rotate position if necessary
            float currentRotation = rotateSpeed * timePassed;
            Vector2 aim = Game1.RotateVector(startAim, currentRotation);

            // update position to match user
            Midpoint = user.Midpoint + (user.Width + this.Width)/ 2f * aim;

            // check hit using circles
            if(user is Player) {
                List<Enemy> enemies = level.Enemies;
                foreach(Enemy enemy in enemies) {
                    if(HitCircle.Intersects(enemy.HitCircle) && !hitEntities.Contains(enemy)) {
                        enemy.TakeDamage(level);
                        hitEntities.Add(enemy);
                    }
                }
            } else { // user is an enemy
                if(HitCircle.Intersects(Game1.Player.HitCircle) && !hitEntities.Contains(Game1.Player)) {
                    Game1.Player.TakeDamage(level);
                    hitEntities.Add(Game1.Player);
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            Game1.RotateDraw(sb, currentAnimation.CurrentSprite, DrawBox, Color.White, drawRotation, flips);
        }
    }
}

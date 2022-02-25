using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Mushroom : Enemy
    {
        // each entity should copy from these
        public static readonly Animation Shoot = new Animation(Graphics.Mushroom, AnimationType.Rebound, 0.05f, true);
        public static readonly Animation Hide = new Animation(Graphics.MushroomHide, AnimationType.Hold, 0.01f);
        public static readonly Particle SporeCloud = new Particle(new Rectangle(0, 0, 100, 100), Graphics.SporeBurst, 0.25f);

        private bool blocking;
        private float blockTimer;

        public Mushroom(int x, int y) : base(EnemyTypes.Mushroom, new Vector2(x, y), 50, 50, 2, 0) {
            timer = 2;
            blocking = false;
            blockTimer = 0f;
            sprite = null;
            currentAnimation = new Animation(Shoot);
            drawBox.Inflate(5, 5); // make mushroom appear larger
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            currentAnimation.Update(deltaTime);

            if(blocking) {
                blockTimer -= deltaTime;

                // end block
                if(blockTimer <= 0) {
                    blocking = false;
                    currentAnimation = new Animation(Hide, true);
                    currentAnimation.Next = new Animation(Shoot);
                    blockTimer = -6f; // cooldown
                }
            } else {
                tint = Color.White;
                if(timer > 0) {
                    timer -= deltaTime;
                }
                if(timer <= 0 && DistanceTo(Game1.Player) <= 900) { // range
                    // fire
                    timer = 2f; // shoot cooldown
                    Vector2 aim = Game1.Player.Midpoint - Midpoint;
                    aim.Normalize();
                    level.Abilities.Add(new BounceSpore(Midpoint, aim, false));

                    currentAnimation.Restart(); // restart shoot animation to animate
                    level.Particles.Add(new Particle(SporeCloud, Midpoint - new Vector2(0, 25)));
                }

                // block
                if(blockTimer < 0) {
                    blockTimer += deltaTime;
                } else {
                    foreach(Entity projectile in level.Abilities) {
                        if(projectile is Projectile && ((Projectile)projectile).FromPlayer && DistanceTo(projectile) <= 150f) {
                            blocking = true;
                            currentAnimation = new Animation(Hide);
                            blockTimer = 0.5f; // block duration
                        }
                    }
                }
            }
        }

        public override void TakeDamage(Level level, int damage = 1) {
            if(!blocking) {
                base.TakeDamage(level, damage);
            } else {
                level.Particles.Add(new Particle(SporeCloud, Midpoint - new Vector2(0, 25)));
            }
        }
    }
}

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
        public static readonly Animation Shoot = new Animation(Graphics.Mushroom, AnimationType.Rebound, 0.05f, true);
        public static readonly Particle SporeCloud = new Particle(new Rectangle(0, 0, 100, 100), Graphics.SporeBurst, 0.25f);

        private bool blocking;
        private float blockTimer;

        public Mushroom(int x, int y) : base(EnemyTypes.Mushroom, new Vector2(x, y), 50, 50, 2, 0) {
            timer = 2;
            blocking = false;
            blockTimer = 0f;
            sprite = null;
            currentAnimation = Shoot;
            drawBox.Inflate(5, 5); // make mushroom appear larger
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            currentAnimation.Update(deltaTime);

            if(blocking) {
                tint = Color.Pink;
                blockTimer -= deltaTime;
                if(blockTimer <= 0) {
                    blocking = false;
                    blockTimer = -3f; // cooldown
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
                    level.Projectiles.Add(new BounceSpore(Midpoint, aim, false));

                    currentAnimation.Restart();
                    level.Particles.Add(new Particle(SporeCloud, Midpoint - new Vector2(0, 25)));
                }

                // block
                if(blockTimer < 0) {
                    blockTimer += deltaTime;
                } else {
                    foreach(Projectile projectile in level.Projectiles) {
                        if(projectile.FromPlayer && DistanceTo(projectile) <= 150f) {
                            blocking = true;
                            blockTimer = 2f; // block duration
                        }
                    }
                }
            }
        }

        // temporary
        public override void Draw(SpriteBatch sb) {
            base.Draw(sb);
            if(blocking) {
                sb.Draw(currentAnimation.CurrentSprite, DrawBox, Color.Pink);
            }
        }

        public override void TakeDamage(int damage) {
            if(!blocking) {
                base.TakeDamage(damage);
            }
        }
    }
}

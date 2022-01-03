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
        //public const int SPORE_SPEED = 800;
        //public const int SPORE_SIZE = 20;
        public static readonly Animation Shoot = new Animation(Graphics.Mushroom, AnimationType.Rebound, 0.05f, true);
        public static readonly Particle SporeCloud = new Particle(new Rectangle(0, 0, 100, 100), Graphics.SporeBurst, 0.25f); // -50
        public static readonly Projectile Spore = new Projectile(800, 20, Graphics.Spore,
            new Particle(new Rectangle(0, 0, 20, 20), Graphics.SporeBreak, 0.1f),
            new Particle(new Rectangle(0, 0, 20, 20), Graphics.SporeTrail, 0.1f)
        );

        private bool blocking;
        private float blockTimer;

        public Mushroom(int x, int y) : base(EnemyTypes.Mushroom, x, y, 50, 50, 2) {
            timer = 2;
            blocking = false;
            blockTimer = 0f;
            sprite = null;
            currentAnimation = Shoot;
            drawBox = new Rectangle(-5, -5, width + 10, height + 10); // make mushroom appear larger
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
                if(timer <= 0 && DistanceTo(Game1.Player) <= 500) { // range
                    // fire
                    timer = 1.5f;
                    Vector2 aim = Game1.Player.Midpoint - Midpoint;
                    aim.Normalize();
                    level.Projectiles.Add(new Projectile(Spore, Midpoint, aim, false));

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

        public override void TakeDamage(int damage) {
            if(!blocking) {
                base.TakeDamage(damage);
            }
        }
    }
}

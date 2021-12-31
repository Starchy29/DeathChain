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
        public const int SPORE_SPEED = 800;
        public const int SPORE_SIZE = 20;

        private bool blocking;
        private float blockTimer;

        public Mushroom(int x, int y) : base(EnemyTypes.Mushroom, x, y, 50, 50, 2) {
            timer = 2;
            blocking = false;
            blockTimer = 0f;
            sprite = Graphics.Mushroom;
            drawBox = new Rectangle(-5, -5, width + 10, height + 10); // make mushroom appear larger
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
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
                if(timer <= 0 && DistanceTo(Game1.Player) <= 700) { // range
                    // fire
                    timer = 1.5f;
                    Vector2 aim = Game1.Player.Midpoint - Midpoint;
                    aim.Normalize();
                    level.Projectiles.Add(new Projectile(Midpoint, aim * SPORE_SPEED, false, SPORE_SIZE, Graphics.Spore));
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

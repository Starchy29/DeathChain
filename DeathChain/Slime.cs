using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Slime : Enemy
    {
        public const int MAX_SPEED = 180;
        public static readonly Projectile SLIMEBALL = new Projectile(500, 500, 30, Graphics.SlimeBall);

        private float wanderTime;
        private float puddleTime;
        private Random rng;

        public Slime(int x, int y) : base(EnemyTypes.Slime, new Vector2(x, y), 50, 50, 3, MAX_SPEED) {
            rng = new Random();
            timer = 3f;
            puddleTime = 0f;
            sprite = Graphics.Slime;
            drawBox = new Rectangle(0, -10, 50, 60);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // wander
            wanderTime -= deltaTime;
            if(wanderTime <= 0) {
                ChangeDirection();
            }

            if(timer < 0.5f) { // freeze when about to shoot
                direction = Vector2.Zero;
            }

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                ChangeDirection();
            }

            // attacks
            timer -= deltaTime;
            if(timer <= 0) {
                timer = 3f; // cooldown
                level.Projectiles.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(1, 0), false));
                level.Projectiles.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(-1, 0), false));
                level.Projectiles.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, 1), false));
                level.Projectiles.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, -1), false));
                ChangeDirection(); // start moving again
            }

            puddleTime -= deltaTime;
            if(puddleTime <= 0) {
                puddleTime = SlimePuddle.DURATION; // cooldown
                level.Projectiles.Add(new SlimePuddle(Midpoint, false));
            }
        }

        private void ChangeDirection() {
            wanderTime = 1f; // how often this changes direction
            direction = new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f);
        }
    }
}

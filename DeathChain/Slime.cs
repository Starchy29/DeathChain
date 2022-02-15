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
        public const float PUDDLE_DURATION = 6f;
        public const int MAX_SPEED = 180;
        public static readonly Projectile SLIMEBALL = new Projectile(500, 500, 30, Graphics.SlimeBall);
        public static readonly Zone SlimePuddle = new Zone(Vector2.Zero, false, 60, PUDDLE_DURATION, 0.5f, new Texture2D[] {Graphics.Button}, true, null);

        private float puddleTime;

        public Slime(int x, int y) : base(EnemyTypes.Slime, new Vector2(x, y), 50, 50, 3, MAX_SPEED) {
            timer = 1f + (float)Game1.RNG.NextDouble() * 4f;
            puddleTime = 0f;
            sprite = Graphics.Slime;
            drawBox = new Rectangle(-5, -20, 60, 70);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // wander
            moveTimer -= deltaTime;
            if(moveTimer <= 0) {
                ChangeDirection();
            }

            if(timer < 0.2f) { // freeze when about to shoot
                direction = Vector2.Zero;
            }

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                ChangeDirection();
            }

            // attacks
            timer -= deltaTime;
            if(timer <= 0) {
                timer = 1f + (float)Game1.RNG.NextDouble(); // shoot cooldown
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(1, 0), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(-1, 0), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, 1), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, -1), false));
                ChangeDirection(); // start moving again
            }

            puddleTime -= deltaTime;
            if(puddleTime <= 0) {
                puddleTime = PUDDLE_DURATION; // cooldown
                level.Abilities.Add(new Zone(SlimePuddle, Midpoint, false));
            }
        }
    }
}

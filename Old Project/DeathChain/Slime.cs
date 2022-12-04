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
        public const int MAX_SPEED = 180;                        // speed, range, size
        public static readonly Projectile SLIMEBALL = new Projectile(600, 500, 30, new Texture2D[] { Graphics.SlimeBall });
        public static readonly Zone SlimePuddle = new Zone(Vector2.Zero, false, 60, PUDDLE_DURATION, 0.5f, Graphics.SlimePuddle, true, null);

        public Slime(int x, int y, int difficulty) : base(EnemyTypes.Slime, new Vector2(x, y), 50, 50, 3, MAX_SPEED, difficulty) {
            timer = 1f + (float)Game1.RNG.NextDouble() * 4f;
            sprite = Graphics.Slime;
            drawBox = new Rectangle(-5, -20, 60, 70);

            startupDuration = 0.2f;
            cooldownDuration = 1.5f; // changes each attack, average
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // wander
            moveTimer -= deltaTime;
            if(moveTimer <= 0) {
                ChooseRandomDirection();
            }

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                ChooseRandomDirection();
            }

            if(OffCooldown()) {
                Attack();
            }
        }

        protected override void AttackEffects(Level level) {
            cooldownDuration = 1f + (float)Game1.RNG.NextDouble(); // shoot cooldown
            ChooseRandomDirection(); // start moving again

            if(Game1.RNG.NextDouble() < 0.3) {
                // 30% chance puddle
                level.Abilities.Add(new Zone(SlimePuddle, Midpoint, false));
            } else {
                // 70% chance slimeballs
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(1, 0), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(-1, 0), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, 1), false));
                level.Abilities.Add(new Projectile(SLIMEBALL, Midpoint, new Vector2(0, -1), false));
            }
        }
    }
}

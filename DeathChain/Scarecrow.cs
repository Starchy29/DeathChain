using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Scarecrow : Enemy
    {
        public const int BURST_RANGE = 150;
        public static Explosion FlameBurst = new Explosion(Vector2.Zero, false, 70, 0.2f, new Texture2D[] { Graphics.Button });

        public Scarecrow(int x, int y) : base(EnemyTypes.Scarecrow, new Vector2(x, y), 50, 50, 3, 0) {
            sprite = Graphics.Scarecrow;
            drawBox.Inflate(20, 20);
            drawBox.Offset(0, -10);
            timer = (float)Game1.RNG.NextDouble() * 3f + 1f;
            timer = (float)Game1.RNG.NextDouble() * 3f + 1f;

            startupDuration = 0.1f;
            cooldownDuration = 1.5f; // changes depending on attack used
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer > 0) {
                timer -= deltaTime;
            }
            if(timer <= 0) {
                // teleport if not in a good position
                Rectangle bounds = level.Bounds;
                bounds.Inflate(-200, -200);

                Vector2 toPlayer = Game1.Player.Midpoint - Midpoint;
                float playDist = toPlayer.Length();
                if(toPlayer != Vector2.Zero) {
                    toPlayer.Normalize();
                }

                if(playDist > 400) {
                    // teleport closer
                    Midpoint += toPlayer * Game1.RNG.Next(200, (int)playDist - 200);
                } else {
                    // teleport away
                    Midpoint += -toPlayer * Game1.RNG.Next(100, 300);
                }

                timer = 5f + (float)Game1.RNG.NextDouble() * 3f;            
            }

            CheckWallCollision(level, true); // move outside of walls

            // attack
            if(OffCooldown()) {
                Attack();
            }
        }

        protected override void AttackEffects(Level level) {
            if(DistanceTo(Game1.Player) <= 250f) {
                // flame burst
                cooldownDuration = 1.5f;
                Vector2 direction = Game1.Player.Midpoint - Midpoint;
                if(direction != Vector2.Zero) {
                    direction.Normalize();
                }
                level.Abilities.Add(new Explosion(FlameBurst, Midpoint + direction * BURST_RANGE, false));
            } else {
                // flame spiral
                cooldownDuration = 3f;
                level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2(1, 0), false));
                level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2((float)Math.Cos(2 * Math.PI / 3), (float)Math.Sin(2 * Math.PI / 3)), false));
                level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2((float)Math.Cos(-2 * Math.PI / 3), (float)Math.Sin(-2 * Math.PI / 3)), false));
            }
        }
    }
}

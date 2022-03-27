using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Blight : Enemy
    {
        public const int MAX_SPEED = 450;
        public const int EXPLOSION_RADIUS = 110;
        public const float STARTUP = 0.1f;
        private List<Vector2> directionOptions;

        public Blight(int x, int y) : base(EnemyTypes.Blight, new Vector2(x, y), 50, 50, 2, MAX_SPEED) {
            sprite = Graphics.Blight;
            drawBox.Inflate(5, 15);
            drawBox.Offset(0, -15);
            timer = 1f + (float)Game1.RNG.NextDouble() * 3f;
            moveTimer = 0f;

            directionOptions = new List<Vector2>();

            Vector2 diagonal = new Vector2(1f, 1f);
            diagonal.Normalize();

            directionOptions.Add(diagonal);
            directionOptions.Add(new Vector2(-diagonal.X, diagonal.Y));
            directionOptions.Add(new Vector2(diagonal.X, -diagonal.Y));
            directionOptions.Add(new Vector2(-diagonal.X, -diagonal.Y));

            startupDuration = 0.7f;
            cooldownDuration = 3f; // changes every explosion, averages 3
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // explode sometimes
            if(OffCooldown()) {
                Attack();
            }

            // change directions sometimes
            moveTimer -= deltaTime;
            if(moveTimer <= 0) {
                ChooseRandomDirection(directionOptions);
            }

            //PassWalls(level);

            List<Direction> collisions = CheckWallCollision(level, true);
            if(timer > 0 && collisions.Count > 0) {
                ChooseRandomDirection(directionOptions);
            }
        }

        protected override void AttackEffects(Level level) {
            cooldownDuration = 2f + (float)Game1.RNG.NextDouble() * 2f; // time until next explosion
            ChooseRandomDirection(directionOptions);
            level.Abilities.Add(new Explosion(Midpoint, false, EXPLOSION_RADIUS, STARTUP, Graphics.BlightExplosion));
        }
    }
}

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
        public const int MAX_SPEED = 500;
        //private const float COOLDOWN = -1f;
        public const int EXPLOSION_RADIUS = 110;
        public const float STARTUP = 0.1f;
        private List<Vector2> directionOptions;

        public Blight(int x, int y) : base(EnemyTypes.Blight, new Vector2(x, y), 50, 50, 1, MAX_SPEED) {
            sprite = Graphics.Blight;
            drawBox.Inflate(6, 6);
            drawBox.Offset(0, -3);
            timer = 1f + (float)Game1.RNG.NextDouble() * 3f;
            moveTimer = 0f;

            directionOptions = new List<Vector2>();

            Vector2 diagonal = new Vector2(1f, 1f);
            diagonal.Normalize();

            directionOptions.Add(diagonal);
            directionOptions.Add(new Vector2(-diagonal.X, diagonal.Y));
            directionOptions.Add(new Vector2(diagonal.X, -diagonal.Y));
            directionOptions.Add(new Vector2(-diagonal.X, -diagonal.Y));
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer < 0)  {
                // pause before explosion
                timer += deltaTime;
                if(timer >= 0) {
                    // explode
                    timer = 2f + (float)Game1.RNG.NextDouble() * 2f; // time until enext explosion
                    ChangeDirection(directionOptions);
                    level.Abilities.Add(new Explosion(Midpoint, false, EXPLOSION_RADIUS, STARTUP, new Texture2D[] { Graphics.Button }));
                }
            }
            else {
                // change directions sometimes
                moveTimer -= deltaTime;
                if(moveTimer <= 0) {
                    ChangeDirection(directionOptions);
                }

                // stop and explode every interval
                timer -= deltaTime;
                if(timer <= 0) {
                    // start pause before explosion
                    timer = -0.7f;
                    direction = Vector2.Zero;
                }
            }

            PassWalls(level);
            List<Direction> collisions = CheckWallCollision(level, true);
            if(timer > 0 && collisions.Count > 0) {
                ChangeDirection(directionOptions);
            }
        }
    }
}

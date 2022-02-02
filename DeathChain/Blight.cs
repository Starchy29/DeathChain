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
        private const float COOLDOWN = -1f;
        public const int EXPLOSION_RADIUS = 110;
        public const float STARTUP = 0.1f;

        public Blight(int x, int y) : base(EnemyTypes.Blight, new Vector2(x, y), 50, 50, 1, MAX_SPEED) {
            sprite = Graphics.Blight;
            drawBox.Inflate(6, 6);
            drawBox.Offset(0, -3);
            timer = 4f;
            moveTimer = 0f;
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer < 0)  {
                // pause after explosion
                timer += deltaTime;
                if(timer >= 0) {
                    timer = 3f;
                    ChangeDirection();
                    level.Abilities.Add(new Explosion(Midpoint, false, EXPLOSION_RADIUS, STARTUP, new Texture2D[] { Graphics.Button }));
                }
            }
            else {
                moveTimer -= deltaTime;
                if(moveTimer <= 0) {
                    ChangeDirection();
                }

                // stop and explode every interval
                timer -= deltaTime;
                if(timer <= 0) {
                    timer = COOLDOWN;
                    direction = Vector2.Zero;
                }
            }

            PassWalls(level);
            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                ChangeDirection();
            }
        }
    }
}

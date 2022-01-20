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
        public const int EXPLOSION_RADIUS = 100;
        public const float STARTUP = 0.1f;

        bool chasing = false;

        public Blight(int x, int y) : base(EnemyTypes.Blight, new Vector2(x, y), 50, 50, 1, MAX_SPEED) {}

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer < 0)  {
                // pause after explosion
                timer += deltaTime;
                if(timer >= 0) {
                    timer = 0;
                }
            }
            else {
                if(chasing) {
                    direction = Game1.Player.Midpoint - Midpoint;

                    // when close to the player, stop and explode
                    if(DistanceTo(Game1.Player) <= 120) {
                        chasing = false;
                        timer = COOLDOWN;
                        direction = Vector2.Zero;
                        level.Abilities.Add(new Explosion(Midpoint, false, EXPLOSION_RADIUS, STARTUP, new Texture2D[]{ Graphics.Button}));
                    }
                } else {
                    // don't chase player until within range
                    if(DistanceTo(Game1.Player) <= 600) {
                        chasing = true;
                    }
                }
            }

            PassWalls(level);
            CheckWallCollision(level, true);
        }
    }
}

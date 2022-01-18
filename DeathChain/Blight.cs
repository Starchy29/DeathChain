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
        public const float EXPLOSION_DURATION = 0.2f;
        private const float COOLDOWN = -1f;
        public const int EXPLOSION_RADIUS = 100;

        bool chasing = false; 

        private bool Exploding { get { return timer <= COOLDOWN + EXPLOSION_DURATION; } }

        public Blight(int x, int y) : base(EnemyTypes.Blight, new Vector2(x, y), 50, 50, 1, MAX_SPEED) {}

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer > 0) {
                // pause before explosion
                timer -= deltaTime;
                if(timer <= 0) {
                    timer = COOLDOWN;
                }
            }
            else if(timer < 0)  {
                // pause after explosion
                timer += deltaTime;
                if(timer >= 0) {
                    timer = 0;
                }

                if(Exploding) {
                    Circle explosion = new Circle(Midpoint, EXPLOSION_RADIUS);
                    if(Game1.Player.HitCircle.Intersects(explosion)) {
                        Game1.Player.TakeDamage(1);
                    }
                }
            }
            else {
                if(chasing) {
                    direction = Game1.Player.Midpoint - Midpoint;

                    // when close to the player, stop and explode
                    if(DistanceTo(Game1.Player) <= 120) {
                        chasing = false;
                        timer = 0.25f; // time before explosion
                        direction = Vector2.Zero;
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

        public override void Draw(SpriteBatch sb) {
            base.Draw(sb);

            if(Exploding && Alive) {
                sb.Draw(Graphics.Button, new Rectangle((int)(Midpoint.X - EXPLOSION_RADIUS + Camera.Shift.X), (int)(Midpoint.Y - EXPLOSION_RADIUS + Camera.Shift.Y), EXPLOSION_RADIUS * 2, EXPLOSION_RADIUS * 2), Color.Orange);
            }
        }
    }
}

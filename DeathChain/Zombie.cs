using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Zombie : Enemy
    {
        public const int MAX_SPEED = 150;
        public const float LUNGE_DURATION = 0.1f;
        public const int LUNGE_SPEED = 1500;

        private const float ATTACK_COOLDOWN = 2f;

        private bool lunging; // only 2 states
        private float slashTime; // time left in slash
        private Random rng;
        private Rectangle slashBox;

        public Zombie(int x, int y) : base(EnemyTypes.Zombie, new Vector2(x, y), 50, 50, 3, MAX_SPEED) {
            sprite = Graphics.Zombie;
            lunging = false;
            rng = new Random(x * y);
            drawBox.Inflate(20, 16);
            drawBox.Offset(0, -15);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(lunging) {
                if(timer < 0) {
                    // pausing
                    timer += deltaTime;
                    if(timer >= 0) {
                        if(rng.NextDouble() <= 0.4) {
                            // lunge
                            timer = LUNGE_DURATION;
                            direction = Game1.Player.Midpoint - Midpoint;
                            if(direction != Vector2.Zero) {
                                direction.Normalize();
                            }
                            velocity = direction * LUNGE_SPEED;
                            maxSpeed = LUNGE_SPEED;
                        } else {
                            // slash
                            lunging = false;
                            timer = ATTACK_COOLDOWN;
                            slashTime = 0.2f;
                        }
                    }
                }
                else {
                    // lunging
                    timer -= deltaTime;
                    if(timer <= 0) {
                        // end lunge
                        timer = ATTACK_COOLDOWN; // don't lunge again for at least this time
                        lunging = false;
                        velocity = Vector2.Zero;
                        maxSpeed = MAX_SPEED;
                    }
                }
            } else {
                // move towards player
                direction = Game1.Player.Midpoint - Midpoint;

                PassWalls(level);
                Separate(level, deltaTime); // move away from other enemies

                // chance to lunge when close enough
                if(timer > 0) {
                    timer -= deltaTime;
                }

                if(timer <= 0) {
                    timer += 0.4f; // how often it checks whether or not to attack
                    if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= 180) { // attack player if close enough
                        if(rng.NextDouble() <= 0.7) {
                            // begin attack
                            timer = -0.4f; // pause time at start of attack
                            velocity = Vector2.Zero;
                            direction = Vector2.Zero;
                            lunging = true;
                        }
                    }
                }
            }

            // slash check
            if(slashTime > 0) {
                slashTime -= deltaTime;
                if(slashTime <= 0) {
                    timer = 2; // attack cooldown
                }

                slashBox = Hitbox;
                Vector2 aim = Game1.Player.Midpoint - Midpoint;
                if(aim.Length() > 0) {
                    aim.Normalize();
                }
                slashBox.Offset(aim * 50);

                // check if hit player
                if(slashBox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamage(level);
                }
            }

            CheckWallCollision(level, true);
        }

        public override void Draw(SpriteBatch sb) {
            base.Draw(sb);

            // draw slash
            slashBox.Offset(Camera.Shift);
            if(alive && slashTime > 0 && slashBox != null) {
                sb.Draw(Graphics.Slash, slashBox, Color.White);
            }
        }
    }
}

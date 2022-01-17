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

        public Zombie(int x, int y) : base(EnemyTypes.Zombie, new Vector2(x, y), 50, 50, 3) {
            sprite = Graphics.Zombie;
            lunging = false;
            rng = new Random(x * y);
            drawBox = new Rectangle(0, -15, 50, 65);
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
                            Vector2 direction = Game1.Player.Midpoint - Midpoint;
                            direction.Normalize();
                            velocity = direction * LUNGE_SPEED;
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
                    //position += velocity * deltaTime;
                    
                    timer -= deltaTime;
                    if(timer <= 0) {
                        // end lunge
                        timer = ATTACK_COOLDOWN; // don't lunge again for at least this time
                        lunging = false;
                        velocity = Vector2.Zero;
                    }
                }
            } else {
                // move towards player
                Vector2 direction = Game1.Player.Midpoint - Midpoint;
                if(direction.Length() > 0) {
                    direction.Normalize();
                }

                // move around walls
                Rectangle future = Hitbox;
                future.Offset(direction * 40);
                foreach(Wall wall in level.Walls) {
                    if(wall.Hitbox.Intersects(future)) { // about to move into wall
                        Vector2 newDirection = wall.Midpoint - Midpoint; // direction from this to wall center
                        newDirection.X /= wall.Width; // factor in wall dimensions
                        newDirection.Y /= wall.Height;
                        newDirection.Normalize();
                        newDirection = new Vector2(newDirection.Y, -newDirection.X); // now perpendicular to wall center
                        if(Vector2.Dot(direction, newDirection) < 0) {
                            newDirection *= -1; // use other perpendicular direction because it is closer
                        }
                        direction = newDirection;
                        break;
                    }
                }
                
                if(Vector2.Dot(direction, velocity) >= 0) { // don't approach player when knocked back
                    // move
                    velocity += direction * deltaTime * ACCEL;

                    // cap speed
                    if(velocity.Length() > MAX_SPEED) {
                        velocity.Normalize();
                        velocity *= MAX_SPEED;
                    }
                }

                Separate(level, deltaTime); // move away from other enemies
                ApplyFriction(deltaTime);

                //position += velocity * deltaTime;

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
                    Game1.Player.TakeDamage(1);
                }
            }

            position += velocity * deltaTime;

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

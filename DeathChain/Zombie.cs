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
        private bool lunging; // only 2 states
        private Random rng;

        public Zombie(int x, int y) : base(EnemyTypes.Zombie, x, y, 50, 50, 3) { 
            tint = Color.Brown;
            lunging = false;
            rng = new Random(x * y);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(lunging) {
                if(timer < 0) {
                    // pausing
                    timer += deltaTime;
                    if(timer >= 0) {
                        timer = 0.1f; // lunge duration
                        Vector2 direction = Game1.Player.Midpoint - Midpoint;
                        direction.Normalize();
                        velocity = direction * 1500; // lunge speed
                    }
                }
                else {
                    // lunging
                    position += velocity * deltaTime;
                    
                    timer -= deltaTime;
                    if(timer <= 0) {
                        // end lunge
                        timer = 2; // don't lunge again for at least this time
                        lunging = false;
                        velocity = Vector2.Zero;
                    }
                }
            } else {
                // move toward player
                Vector2 direction = Game1.Player.Midpoint - Midpoint;
                if(DistanceTo(Game1.Player) > 800) {
                    // too far: wander instead
                    direction = velocity;
                    if(direction == Vector2.Zero) {
                        direction = new Vector2((float)rng.NextDouble() * 2 - 1, (float)rng.NextDouble() * 2 - 1);
                    }
                    direction = Vector2.Transform(direction, Matrix.CreateRotationZ((float)rng.NextDouble() * 2 - 1));
                }
                if(direction.Length() > 0) {
                    direction.Normalize();
                }
                velocity += direction * 2000 * deltaTime;
                if(Vector2.Dot(direction, velocity) > 0 && velocity.Length() > 200) {
                    velocity.Normalize();
                    velocity *= 200;
                }

                // move away from other enemies
                foreach(Enemy enemy in level.Enemies) {
                    if(enemy != this && Vector2.Distance(Midpoint, enemy.Midpoint) <= 100) {
                        Vector2 moveAway = Midpoint - enemy.Midpoint;
                        velocity += moveAway * 10 * deltaTime;
                    }
                }

                // apply friction
                Vector2 friction = -velocity;
                if(friction != Vector2.Zero) {
                    friction.Normalize();
                    velocity += friction * deltaTime * 1000;
                    if(Vector2.Dot(friction, velocity) > 0) {
                        // started moving backwards: stop instead
                        velocity = Vector2.Zero;
                    }
                }
                

                position += velocity * deltaTime;

                // chance to lunge when close enough
                timer -= deltaTime;
                if(timer <= 0) {
                    timer += 0.5f; // this is how often it checks whether or not to lunge
                    if(rng.NextDouble() <= 0.3 && Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= 150) {
                        // begin lunge 
                        timer = -0.4f; // pause time at start of lunge
                        velocity = Vector2.Zero;
                        lunging = true;;
                    }
                }
            }

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Contains(Direction.Left) || collisions.Contains(Direction.Right)) {
                velocity.X = 0;
            }
            if(collisions.Contains(Direction.Up) || collisions.Contains(Direction.Down)) {
                velocity.Y = 0;
            }
        }
    }
}

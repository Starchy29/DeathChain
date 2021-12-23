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

        public Zombie(int x, int y) : base(x, y, 50, 50, 3) { 
            tint = Color.Brown;
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // move toward player
            Vector2 direction = Game1.Player.Midpoint - Midpoint;
            direction.Normalize();
            velocity += direction * 1000 * deltaTime;
            if(Vector2.Dot(direction, velocity) > 0 && velocity.Length() > 200) {
                velocity.Normalize();
                velocity *= 200;
            }
            position += velocity * deltaTime;

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                velocity = Vector2.Zero;
            }
        }
    }
}

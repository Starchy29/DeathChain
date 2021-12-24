using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Projectile : Entity
    {
        private bool fromPlayer;
        public bool FromPlayer { get { return fromPlayer; } }

        public Projectile(Vector2 midpoint, Vector2 velocity, bool fromPlayer, int length)
            : base((int)midpoint.X - length / 2, (int)midpoint.Y - length / 2, length, length, Graphics.Pixel) {
            this.velocity = velocity;
            this.fromPlayer = fromPlayer;
        }

        public override void Update(Level level, float deltaTime) {
            position += velocity * deltaTime;

            List<Direction> collisions = CheckWallCollision(level, false);
            if(collisions.Count > 0) {
                IsActive = false;
            }

            // check if hit a target
            if(fromPlayer) {
                foreach(Enemy enemy in level.Enemies) {
                    if(enemy.Alive && Collides(enemy)) {
                        enemy.TakeDamage(1);
                        enemy.Push(velocity);
                        IsActive = false;
                    }
                }
            } else {
                if(Collides(Game1.Player)) {
                    Game1.Player.TakeDamage(1);
                    Game1.Player.Push(velocity);
                    IsActive = false;
                }
            }
        }
    }
}

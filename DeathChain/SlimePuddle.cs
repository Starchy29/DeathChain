using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    // puddle of slime that damages a character when they walk into it
    class SlimePuddle : Projectile // loose definition of projectile
    {
        public const float DURATION = 8f;

        private float timeLeft;
        private Circle collision;

        public SlimePuddle(Vector2 midpoint, bool fromPlayer) 
            : base(midpoint, Vector2.Zero, 1, 80, fromPlayer, Graphics.Button) 
        {
            timeLeft = DURATION;
            collision = new Circle(midpoint, 35); // a little less than half the length
            tint = Color.Purple;
        }

        public override void Update(Level level, float deltaTime) {
            // check for collision (except for a moment at beginning to allow reaction time)
            if(timeLeft < DURATION - 0.5f) {
                if(fromPlayer) {
                    foreach(Enemy enemy in level.Enemies) {
                        if(enemy.Alive && collision.Intersects(enemy.HitCircle)) {
                            enemy.TakeDamage(1);
                            IsActive = false;
                        }
                    }
                } else {
                    if(collision.Intersects(Game1.Player.HitCircle)) {
                        Game1.Player.TakeDamage(1);
                        IsActive = false;
                    }
                }
            }

            // disappear after some time
            timeLeft -= deltaTime;
            if(timeLeft <= 0) {
                IsActive = false;
            }
        }
    }
}

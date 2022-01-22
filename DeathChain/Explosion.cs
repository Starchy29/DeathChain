using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Explosion : Zone
    {
        private List<Enemy> hitEnemies;
        private bool hitPlayer;

        public Explosion(Vector2 midpoint, bool fromPlayer, int radius, float startup, Texture2D[] sprites) 
            : base(midpoint, fromPlayer, radius, startup + 0.2f, startup,sprites, false, null)
        {
            // damage each entity only once
            hitEnemies = new List<Enemy>();
            hitPlayer = false;
        }

        protected override void OnHit(Level level, Enemy enemy = null) {
            if(fromPlayer) { 
                if(enemy != null && !hitEnemies.Contains(enemy)) {
                    enemy.TakeDamage(level);
                    hitEnemies.Add(enemy);
                }
            } else {
                if(!hitPlayer) {
                    Game1.Player.TakeDamage(level);
                    hitPlayer = true;
                }
            }
            // same as default, but don't disappear when hit something
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    class Spider : Enemy
    {
        public const int WEB_SPEED = 600;
        public const int WEB_SIZE = 40;

        private Vector2 orientation; // the direction the spider is facing

        public Spider(int x, int y) : base(EnemyTypes.Spider, x, y, 75, 75, 4) { 
            orientation = new Vector2(0, 1);

            tint = Color.DarkGray;
        }

        // stay still when player is far, attack when they get close, drop web sometimes
        protected override void AliveUpdate(Level level, float deltaTime) {
            
            CheckWallCollision(level, true);
        }
    }
}

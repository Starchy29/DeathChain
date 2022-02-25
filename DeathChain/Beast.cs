using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Beast : Enemy
    {
        public const int MAX_SPEED = 150;

        public Beast(int x, int y) : base(EnemyTypes.Beast, new Vector2(x, y), 100, 100, 4, MAX_SPEED) {
            sprite = Graphics.Pixel;

            // image is 100x150
            drawBox.Inflate(0, 25);
            drawBox.Offset(0, -25);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // slowly approach player when close
            // wander when far?
            // rush
            // slash
        }
    }
}

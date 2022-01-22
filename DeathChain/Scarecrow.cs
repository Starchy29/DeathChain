using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class Scarecrow : Enemy
    {
        Random rng;

        public Scarecrow(int x, int y) : base(EnemyTypes.Scarecrow, new Vector2(x, y), 50, 50, 3, 0) {
            rng = new Random(x - y);
            sprite = Graphics.Scarecrow;
            drawBox.Inflate(20, 20);
            drawBox.Offset(0, -10);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            // teleport if not in a good position
            Rectangle bounds = level.Bounds;
            bounds.Inflate(-200, -200);

            // attack (flame burst or spiral flames)
        }
    }
}

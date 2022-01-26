using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    class SpiralFlame : Projectile
    {
        private const int SPEED = 400;
        private Vector2 pivot;

        public SpiralFlame(Vector2 midpoint, Vector2 aim, bool fromPlayer) 
            : base(midpoint, aim, 1600, 50, fromPlayer, Graphics.Pixel, null, null)
        { 
            pivot = midpoint;
        }

        public override void Update(Level level, float deltaTime) {
            // curve
            Vector2 turn = pivot - Midpoint;
            if(turn != Vector2.Zero) {
                turn = new Vector2(turn.Y, -turn.X);
                turn.Normalize();
                turn *= 200;
                velocity += turn;
                velocity.Normalize();
                velocity *= SPEED;
            }

            base.Update(level, deltaTime);
        }
    }
}

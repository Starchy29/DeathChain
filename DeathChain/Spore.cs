using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    // projectile fired by mushrooms
    class Spore : Projectile
    {
        private const int SPEED = 800;

        public Spore(Vector2 midpoint, Vector2 aim, bool fromPlayer) : base(midpoint, aim, fromPlayer, 20) {
            aim.Normalize();
            velocity = aim * SPEED;
        }
    }
}

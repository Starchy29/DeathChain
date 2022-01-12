using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DeathChain
{
    public struct Circle
    {
        private Vector2 middle;
        private float radius;

        public Circle(Vector2 middle, float radius) {
            this.middle = middle;
            this.radius = radius;
        }

        public bool Intersects(Circle other) {
            return Vector2.DistanceSquared(middle, other.middle) < (radius + other.radius) * (radius + other.radius);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Wall : Entity
    {
        private bool isPit; // pits behave slightly differently than normal walls

        public bool IsPit { get { return isPit; } }

        public Wall(int x, int y, int width, int height, bool isPit) : base(new Vector2(x + width / 2, y + height / 2), width, height, Graphics.Pixel) {
            this.isPit = isPit;
            if(isPit) {
                tint = Color.Black;
            } else {
                tint = Color.Gray;
            }
        }
    }
}

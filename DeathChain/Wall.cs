using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathChain
{
    public class Wall : Entity
    {
        private bool isPit; // pits behave slightly differently than normal walls

        public bool IsPit { get { return isPit; } }

        public Wall(int x, int y, int width, int height, bool isPit) : base(x, y, width, height, Graphics.Pixel) {
            this.isPit = isPit;
        }
    }
}

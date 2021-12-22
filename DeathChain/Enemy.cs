using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathChain
{
    public abstract class Enemy : Entity
    {
        public Enemy(int x, int y, int width, int height) : base(x, y, width, height) { }
    }
}

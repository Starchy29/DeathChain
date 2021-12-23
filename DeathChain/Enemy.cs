using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathChain
{
    public abstract class Enemy : Entity
    {
        protected int health;

        public Enemy(int x, int y, int width, int height) : base(x, y, width, height) { }

        public void TakeDamage(int damage) {
            health -= damage;
            if(health <= 0) {
                // die
                active = false;
            }
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathChain
{
    public enum EnemyTypes {
        None,

    }

    class Player : Entity
    {
        private EnemyTypes possessType; // the type of enemy the player is controlling currently

        public Player() : base(0, 0, 50, 50, null) { }

        public override void Update(Level level) {
            // move

            // ability
        }

        public override void Draw(SpriteBatch sb) {
            // make sprite match the current enemy
            base.Draw(sb);
        }
    }
}

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

    public class Player : Entity
    {
        private EnemyTypes possessType; // the type of enemy the player is controlling currently
        private float walkSpeed;

        public Player() : base(775, 425, 50, 50, Graphics.Pixel) {
            walkSpeed = 15.0f;
        }

        public override void Update(Level level) {
            // move
            position += Input.GetMoveDirection() * walkSpeed;

            // check wall collision
            CheckWallCollision(level, true);

            // ability
        }

        public override void Draw(SpriteBatch sb) {
            // make sprite match the current enemy
            base.Draw(sb);

            // draw ui (health, ability slots)
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    /*class WallClimber : Enemy
    {
        public const int WEB_SPEED = 600;
        public const int WEB_SIZE = 40;

        private Direction facing;

        public WallClimber(int x, int y, Direction facing) : base(EnemyTypes.None, x, y, 50, 50, 2) { 
            this.facing = facing;
            if(facing == Direction.None) {
                this.facing = Direction.Right;
            }

            tint = Color.DarkGray;
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(timer < 0) {
                // pause timer after shooting
                timer += deltaTime;
                if(timer > 0) {
                    timer = 1f; // begin cooldown while able to move
                }
            }
            else {
                if(timer > 0) {
                    // shoot cooldown while moving
                    timer -= deltaTime;
                    if(timer < 0) {
                        timer = 0;
                    }
                } else if((FacingVertical() && Math.Abs(Game1.Player.Midpoint.X - Midpoint.X) <= 60) ||
                    (!FacingVertical() && Math.Abs(Game1.Player.Midpoint.Y - Midpoint.Y) <= 60)
                ) {
                    // fire if player close enough
                    level.Projectiles.Add(new Projectile(Midpoint, ConvertAim() * WEB_SPEED, false, WEB_SIZE, Graphics.Pixel));
                    timer = -1f; // begin frozen part of cooldown
                }

                // move toward player
                if(FacingVertical()) {

                } else {

                }
            }
        }

        // groups the directions into vertical and horizontal
        private bool FacingVertical() {
            return facing == Direction.Up || facing == Direction.Down;
        }

        private Vector2 ConvertAim() {
            switch(facing) {
                case Direction.Up:
                    return new Vector2(0, -1);
                case Direction.Down:
                    return new Vector2(0, 1);
                case Direction.Left:
                    return new Vector2(-1, 0);
                case Direction.Right:
                    return new Vector2(1, 0);
                default:
                    return new Vector2(0, 0);
            }
        }
    }*/
}

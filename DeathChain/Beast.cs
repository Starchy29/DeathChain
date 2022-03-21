using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    class Beast : Enemy
    {
        public const int MAX_SPEED = 150;
        public const int ATTACK_SIZE = 100;
        public const float ATTACK_ANGLE = (float)Math.PI / 2; // total angle travelled, centered at aim
        public const float ATTACK_DURATION = 0.2f;

        private bool rushing;

        public Beast(int x, int y) : base(EnemyTypes.Beast, new Vector2(x, y), 100, 100, 4, MAX_SPEED) {
            sprite = Graphics.Beast;

            // image is 100x150
            drawBox.Inflate(10, 35);
            drawBox.Offset(0, -35);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(rushing) {

            } else {
                // move
                float playerDist = DistanceTo(Game1.Player);
                if(playerDist <= 400) { // player detection range
                    // approach player
                    direction = Game1.Player.Midpoint - Midpoint;
                    PassWalls(level);

                    // attack
                    timer -= deltaTime;
                    if(timer <= 0) {
                        timer += 3f; // attack cooldown
                        if(playerDist < 200) {
                            // slash
                            if(direction != Vector2.Zero) {
                                attack = new Attack(this, ATTACK_SIZE, Game1.RotateVector(direction, -ATTACK_ANGLE / 2f), ATTACK_ANGLE, ATTACK_DURATION, Graphics.SlashEffect);
                            }
                        } else {
                            // rush
                        }
                    }
                } else {
                    // wander
                    moveTimer -= deltaTime;
                    if (moveTimer <= 0) {
                        ChangeDirection();
                    }
                }

                // check swipe attack
                List<Direction> collisions = CheckWallCollision(level, true);
                if(collisions.Count > 0) {
                    ChangeDirection();
                }
            }
        }
    }
}

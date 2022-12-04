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
        public const int RUSH_SPEED = 1000;
        public const int ATTACK_SIZE = 100;
        public const float ATTACK_ANGLE = (float)Math.PI / 2; // total angle travelled, centered at aim
        public const float ATTACK_DURATION = 0.2f;

        private bool rushing;

        public Beast(int x, int y, int difficulty) : base(EnemyTypes.Beast, new Vector2(x, y), 100, 100, 4, MAX_SPEED, difficulty) {
            sprite = Graphics.Beast;

            // image is 100x180
            drawBox.Inflate(10, 50);
            drawBox.Offset(0, -50);

            startupDuration = 0.4f;
            cooldownDuration = 3f;
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(!rushing) {
                if(DistanceTo(Game1.Player) <= 450) { // player detection range
                    // seek player
                    direction = Game1.Player.Midpoint - Midpoint;
                    PassWalls(level);

                    // attack
                    if(OffCooldown()) {
                        Attack();
                    }
                } else {
                    // wander
                    moveTimer -= deltaTime;
                    if(moveTimer <= 0) {
                        ChooseRandomDirection();
                    }
                }
            }

            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                if(rushing) {
                    // determine whether or not to end rush based on how much speed is left
                    if(velocity.LengthSquared() > (RUSH_SPEED - 200) * (RUSH_SPEED - 200)) {
                        // give speed back
                        velocity.Normalize();
                        velocity *= RUSH_SPEED;
                    } else {
                        // end rush
                        rushing = false;
                        maxSpeed = MAX_SPEED;
                    }
                } else {
                    ChooseRandomDirection();
                }
            }
        }

        protected override void AttackEffects(Level level) {
            if(DistanceTo(Game1.Player) < 250) {
                // slash
                Vector2 aim = Game1.Player.Midpoint - Midpoint;
                if(aim != Vector2.Zero) {
                    attack = new Attack(this, ATTACK_SIZE, Game1.RotateVector(aim, -ATTACK_ANGLE / 2f), ATTACK_ANGLE, ATTACK_DURATION, Graphics.SlashEffect);
                }
            } else {
                // rush
                rushing = true;
                maxSpeed = RUSH_SPEED;
                direction = Game1.Player.Midpoint - Midpoint;
                if(direction != Vector2.Zero) {
                    direction.Normalize();
                }
                velocity = direction * RUSH_SPEED / 2; // starting speed
            }
        }
    }
}

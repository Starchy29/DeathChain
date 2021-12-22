using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
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
        private float maxSpeed;
        private const float ACCEL = 10000.0f;
        private float friction = 2000f;
        private Vector2 velocity;
        private int health;
        private double dashTime;

        public Player() : base(775, 425, 50, 50, Graphics.Pixel) {
            maxSpeed = 500.0f;
            velocity = Vector2.Zero;
        }

        public override void Update(Level level, float deltaTime) {
            // move
            if(dashTime > 0) {
                dashTime -= deltaTime;
                position += velocity * deltaTime;
                if(dashTime <= 0) {
                    // go on cooldown
                    dashTime = -0.5f;
                }
            } else {
                Vector2 acceleration = Input.GetMoveDirection() * ACCEL;
                Vector2 frictionVec = -velocity;
                if(frictionVec != Vector2.Zero) {
                    frictionVec.Normalize();
                    acceleration += frictionVec * friction;
                }

                velocity += acceleration * deltaTime;
                if(velocity.Length() <= 60) {
                    velocity = Vector2.Zero;
                }
                else if(velocity.Length() >= maxSpeed) {
                    velocity.Normalize();
                    velocity *= maxSpeed;
                }

                position += velocity * deltaTime;
            }

            // check wall collision
            List<Direction> collisions = CheckWallCollision(level, true);
            if(collisions.Count > 0) {
                velocity = Vector2.Zero;
                dashTime = 0; // stop dashing when hit wall
            }

            // abilities
            if(dashTime < 0) {
                dashTime += deltaTime;
                if(dashTime > 0) {
                    dashTime = 0;
                }
            }
            if(dashTime == 0 && Input.JustPressed(Inputs.Ability1)) {
                // dash
                dashTime = 0.10;
                Vector2 dir = Input.GetMoveDirection();
                dir.Normalize();
                velocity = dir * 2000;
            }
        }

        public override void Draw(SpriteBatch sb) {
            // make sprite match the current enemy
            base.Draw(sb);

            // draw ui (health, ability slots)
        }
    }
}

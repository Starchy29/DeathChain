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

    public enum PlayerState {
        Normal,
        Dash,
        Slash
    }

    public delegate void Action();

    public class Player : Entity
    {
        private EnemyTypes possessType; // the type of enemy the player is controlling currently
        private PlayerState state;
        private const float ACCEL = 10000.0f;
        private float friction = 2000f;
        private Vector2 velocity;
        private int health;
        private double timer;
        private Vector2 aim;
        private Rectangle attackArea;
        private double[] cooldowns; // cooldowns for the 4 abilities. 1: A, 2: X, 3: B, 4: Y
        private Dictionary<EnemyTypes, Action[]> abilities;

        public Player() : base(775, 425, 50, 50, Graphics.Pixel) {
            state = PlayerState.Normal;
            velocity = Vector2.Zero;
            cooldowns = new double[4];
            cooldowns[0] = 0.0f;
            cooldowns[1] = 0.0f;
            cooldowns[2] = 0.0f;
            cooldowns[3] = 0.0f;

            abilities = new Dictionary<EnemyTypes, Action[]>();
            abilities[EnemyTypes.None] = new Action[4] { Dash, Slash, null, null };

            tint = Color.Blue;
        }

        public override void Update(Level level, float deltaTime) {
            bool checkWalls = true;
            bool checkPits = true;

            switch(state) {
                case PlayerState.Normal:
                    Move(deltaTime, 500.0f);
                    break;

                case PlayerState.Dash:
                    if(timer >= 0) {
                        // dashing
                        checkPits = false; // can dash over pits
                        timer -= deltaTime;
                        position += velocity * deltaTime;
                        if(timer <= 0) {
                            // freeze at end
                            timer = -0.1f;
                        }
                    } else {
                        // frozen after dash
                        timer += deltaTime;
                        if(timer >= 0) {
                            timer = 0;
                            state = PlayerState.Normal;
                            cooldowns[0] = 0.5;
                        }
                    }
                    break;

                case PlayerState.Slash:
                    Move(deltaTime, 250.0f);
                    GenerateAttack(50, 50);
                    foreach(Enemy enemy in level.Enemies) {
                        if(enemy.Hitbox.Intersects(attackArea)) {
                            // damage enemy
                        }
                    }

                    timer -= deltaTime;
                    if(timer <= 0) {
                        timer = 0;
                        state = PlayerState.Normal;
                        cooldowns[1] = 0.2;
                    }
                    break;
            }

            // check wall collision
            if(checkWalls) {
                List<Direction> collisions = CheckWallCollision(level, checkPits);
                if(collisions.Count > 0) {
                    velocity = Vector2.Zero;
                }
            }

            // abilities
            if(state == PlayerState.Normal) {
                if(cooldowns[0] <= 0 && Input.JustPressed(Inputs.Ability1) && abilities[possessType][0] != null) {
                    abilities[possessType][0]();
                }
                else if(cooldowns[1] <= 0 && Input.JustPressed(Inputs.Ability2) && abilities[possessType][1] != null) {
                    abilities[possessType][1]();
                }
                else if(cooldowns[2] <= 0 && Input.JustPressed(Inputs.Ability3) && abilities[possessType][2] != null) {
                    abilities[possessType][2]();
                }
                else if(cooldowns[3] <= 0 && Input.JustPressed(Inputs.Ability4) && abilities[possessType][3] != null) {
                    abilities[possessType][3]();
                }
            }

            for(int i = 0; i < 4; i++) {
                if(cooldowns[i] > 0) {
                    cooldowns[i] -= deltaTime;
                    if(cooldowns[i] < 0) {
                        cooldowns[i] = 0;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            // make sprite match the current enemy
            base.Draw(sb);

            switch(state) {
                case PlayerState.Slash:
                    if(attackArea != null) {
                        sb.Draw(Graphics.Pixel, attackArea, Color.Red);
                    }
                    break;
            }

            // draw ui (health, ability slots)
        }

        private void Move(float deltaTime, float maxSpeed) {
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

        private void Dash() {
            state = PlayerState.Dash;
            timer = 0.1;
            velocity = Input.GetAim() * 2000;
        }

        private void Slash() {
            state = PlayerState.Slash;
            timer = 0.2;
            aim = Input.GetAim();

            GenerateAttack(50, 50);
        }

        private void GenerateAttack(int length, int dist) {
            Vector2 rectMid = Midpoint + aim * dist;
            attackArea = new Rectangle((int)rectMid.X - length / 2, (int)rectMid.Y - length / 2, length, length);
        }
    }
}

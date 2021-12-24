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
        Zombie,
        Mushroom
    }

    public enum PlayerState {
        Normal,
        Dash,
        Slash,
        Lunge,
        Block
    }

    public delegate void Action(Level level);

    public class Player : Entity
    {
        public const int SELECT_DIST = 80; // distance from a dead enemy that the player can possess them

        private EnemyTypes possessType; // the type of enemy the player is controlling currently
        private PlayerState state;
        private const float ACCEL = 10000.0f;
        private float friction = 2000f;
        private int health;
        private float timer;
        private float invulnTime; // after getting hit
        private Vector2 aim;
        private Rectangle attackArea;
        private List<Enemy> hitEnemies; // when attacking, makes sure each attack ony hits an enemy once
        private double[] cooldowns; // cooldowns for the 3 abilities. 1: A, 2: X, 3: B, Possess: Y
        private Dictionary<EnemyTypes, Action[]> abilities;

        public Player() : base(775, 425, 50, 50, Graphics.Pixel) {
            state = PlayerState.Normal;
            velocity = Vector2.Zero;
            hitEnemies = new List<Enemy>();
            health = 1;

            cooldowns = new double[3];
            cooldowns[0] = 0.0f;
            cooldowns[1] = 0.0f;
            cooldowns[2] = 0.0f;

            abilities = new Dictionary<EnemyTypes, Action[]>();
            abilities[EnemyTypes.None] = new Action[3] { Dash, Slash, null };
            abilities[EnemyTypes.Zombie] = new Action[3] { Lunge, Slash, null };
            abilities[EnemyTypes.Mushroom] = new Action[3] { Block, FireSpore, null };
        }

        public override void Update(Level level, float deltaTime) {
            bool checkWalls = true;
            bool checkPits = true;

            switch(state) {
                case PlayerState.Normal:
                    float maxSpeed = 500;
                    switch(possessType) {
                        case EnemyTypes.Zombie:
                            maxSpeed = 300;
                            break;
                        case EnemyTypes.Mushroom:
                            maxSpeed = 0;
                            break;
                    }
                    Move(deltaTime, maxSpeed);
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
                            velocity = Vector2.Zero;
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
                        if(enemy.Hitbox.Intersects(attackArea) && !hitEnemies.Contains(enemy)) {
                            // damage enemy
                            enemy.TakeDamage(1);
                            enemy.Push(aim * 500);
                            hitEnemies.Add(enemy);
                        }
                    }

                    timer -= deltaTime;
                    if(timer <= 0) {
                        timer = 0;
                        state = PlayerState.Normal;
                        cooldowns[1] = 0.2;
                        hitEnemies.Clear();
                    }
                    break;

                case PlayerState.Lunge:
                    position += velocity * deltaTime;

                    GenerateAttack(50, 50);
                    foreach(Enemy enemy in level.Enemies) {
                        if(enemy.Alive && enemy.Hitbox.Intersects(attackArea) && !hitEnemies.Contains(enemy)) {
                            // damage enemy
                            enemy.TakeDamage(1);
                            enemy.Push(aim * 800);
                            hitEnemies.Add(enemy);
                            timer = 0; // end attack early
                        }
                    }

                    timer -= deltaTime;
                    if(timer <= 0) {
                        timer = 0;
                        state = PlayerState.Normal;
                        cooldowns[0] = 1;
                        hitEnemies.Clear();
                    }
                    break;

                case PlayerState.Block:
                    timer -= deltaTime;
                    if(timer <= 0 || !Input.IsPressed(Inputs.Ability1)) {
                        // end block
                        state = PlayerState.Normal;
                        timer = 0;
                        cooldowns[0] = 2f;
                    }
                    break;
            }

            // check wall collision
            if(checkWalls) {
                List<Direction> collisions = CheckWallCollision(level, checkPits);
                if(collisions.Contains(Direction.Left) || collisions.Contains(Direction.Right)) {
                    velocity.X = 0;
                }
                if(collisions.Contains(Direction.Up) || collisions.Contains(Direction.Down)) {
                    velocity.Y = 0;
                }
            }

            // abilities
            if(state == PlayerState.Normal) {
                if(cooldowns[0] <= 0 && Input.JustPressed(Inputs.Ability1) && abilities[possessType][0] != null) {
                    abilities[possessType][0](level);
                }
                else if(cooldowns[1] <= 0 && Input.JustPressed(Inputs.Ability2) && abilities[possessType][1] != null) {
                    abilities[possessType][1](level);
                }
                else if(cooldowns[2] <= 0 && Input.JustPressed(Inputs.Ability3) && abilities[possessType][2] != null) {
                    abilities[possessType][2](level);
                }
            }

            // possess enemies
            if(Input.JustPressed(Inputs.Possess)) {
                if(possessType == EnemyTypes.None && state == PlayerState.Normal) {
                    // possess
                    List<Enemy> inRange = new List<Enemy>();
                    foreach(Enemy enemy in level.Enemies) { // find enemies in range
                        if(!enemy.Alive && Vector2.Distance(Midpoint, enemy.Midpoint) <= SELECT_DIST) {
                            inRange.Add(enemy);
                        }
                    }

                    if(inRange.Count > 0) {
                        // find closest enemy that can be selected
                        Enemy possessTarget = inRange[0];
                        for(int i = 1; i < inRange.Count; i++) {
                            if(Vector2.Distance(Midpoint, inRange[i].Midpoint) < Vector2.Distance(Midpoint, possessTarget.Midpoint)) {
                                possessTarget = inRange[i];
                            }
                        }

                        // actually possess now
                        health = possessTarget.MaxHealth;
                        possessTarget.IsActive = false;
                        possessType = possessTarget.Type;
                        position = possessTarget.Position;
                        velocity = Vector2.Zero;
                        width = possessTarget.Width;
                        height = possessTarget.Height;
                        for(int i = 0; i < 3; i++) {
                            cooldowns[i] = 0;
                        }
                    }
                }
                else if(possessType != EnemyTypes.None) {
                    // unpossess
                    health = 1;
                    possessType = EnemyTypes.None;
                    state = PlayerState.Normal;
                }
            }

            // timers
            for (int i = 0; i < 3; i++) {
                if(cooldowns[i] > 0) {
                    cooldowns[i] -= deltaTime;
                    if(cooldowns[i] < 0) {
                        cooldowns[i] = 0;
                    }
                }
            }
            if(invulnTime > 0) {
                invulnTime -= deltaTime;
            }
        }

        public override void Draw(SpriteBatch sb) {
            // make sprite match the current enemy
            tint = Color.Blue;
            if(possessType == EnemyTypes.Zombie) {
                tint = Color.SlateBlue;
            }
            else if(possessType == EnemyTypes.Mushroom) {
                if(state == PlayerState.Block) {
                    tint = Color.LightGreen;
                } else {
                    tint = Color.Green;
                }
            }

            if(invulnTime > 0) {
                tint *= 0.5f;
            }
            base.Draw(sb);

            switch(state) {
                case PlayerState.Lunge:
                case PlayerState.Slash:
                    if(attackArea != null) {
                        sb.Draw(Graphics.Pixel, attackArea, Color.Red);
                    }
                    break;
            }

            // draw ui (health, ability slots)
            for(int i = 0; i < health; i++) {
                sb.Draw(Graphics.Pixel, new Rectangle(30 + i * 60, 30, 50, 50), Color.Red);
            }
        }

        public void TakeDamage(int damage) {
            if(invulnTime <= 0 && state != PlayerState.Block) {
                health -= damage;
                invulnTime = 1.0f;
                if(health <= 0) {
                    // die
                }
            }
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

        private void Dash(Level level) {
            state = PlayerState.Dash;
            timer = 0.1f;
            velocity = Input.GetAim() * 2000;
        }

        private void Slash(Level level) {
            state = PlayerState.Slash;
            timer = 0.2f;
            aim = Input.GetAim();

            GenerateAttack(50, 50);
        }

        private void Lunge(Level level) {
            state = PlayerState.Lunge;
            timer = 0.1f;
            aim = Input.GetAim();
            velocity = aim * 1500;
            GenerateAttack(50, 50);
        }

        private void Block(Level level) {
            state = PlayerState.Block;
            timer = 2f; // max block time
        }

        private void FireSpore(Level level) {
            level.Projectiles.Add(new Spore(Midpoint, Input.GetAim(), true));
            cooldowns[1] = 0.75f;
        }

        // generates an attack area relative to the player. Uses the aim variable
        private void GenerateAttack(int length, int dist) {
            Vector2 rectMid = Midpoint + aim * dist;
            attackArea = new Rectangle((int)rectMid.X - length / 2, (int)rectMid.Y - length / 2, length, length);
        }
    }
}

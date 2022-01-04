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
        Mushroom,
        Spider
    }

    public enum PlayerState {
        Normal,
        Dash,
        Slash,
        Lunge,
        Block
    }

    public delegate void Ability(Level level);

    public class Player : Entity
    {
        public const int SELECT_DIST = 80; // distance from a dead enemy that the player can possess them
        private readonly Rectangle playerDrawBox = new Rectangle(0, -15, 50, 65);
        private readonly Animation forward = new Animation(Graphics.PlayerFront, AnimationType.Loop, 0.1f);
        private readonly Animation side = new Animation(Graphics.PlayerSide, AnimationType.Loop, 0.1f);
        private readonly Animation back = new Animation(Graphics.PlayerBack, AnimationType.Loop, 0.1f);

        private EnemyTypes possessType; // the type of enemy the player is controlling currently
        private PlayerState state;
        private const float ACCEL = 10000.0f;
        private float friction = 2000f;
        private int health;
        private int ghostHealth; // the ghost form keeps health even when forms change
        private float timer;
        private float invulnTime; // after getting hit
        private Vector2 aim;
        private Rectangle attackArea;
        private List<Enemy> hitEnemies; // when attacking, makes sure each attack ony hits an enemy once
        private double[] cooldowns; // cooldowns for the 3 abilities.
        private Dictionary<EnemyTypes, Ability[]> abilities;
        private SpriteEffects flips;
        private Direction wasFacing = Direction.Down;
        private Direction facing = Direction.Down; // help choose which animation to use

        private readonly Dictionary<Ability, Texture2D> abilityIcons;

        public Player() : base(775, 425, 50, 50, Graphics.TempGhost) {
            state = PlayerState.Normal;
            velocity = Vector2.Zero;
            hitEnemies = new List<Enemy>();
            health = 3;
            ghostHealth = 3;
            drawBox = playerDrawBox;

            sprite = null;
            currentAnimation = forward;

            cooldowns = new double[3];
            cooldowns[0] = 0.0f;
            cooldowns[1] = 0.0f;
            cooldowns[2] = 0.0f;

            abilities = new Dictionary<EnemyTypes, Ability[]>();
            abilities[EnemyTypes.None] = new Ability[3] { Slash, Dash, null };
            abilities[EnemyTypes.Zombie] = new Ability[3] { Slash, Lunge, null };
            abilities[EnemyTypes.Mushroom] = new Ability[3] { FireSpore, Block, null };

            abilityIcons = new Dictionary<Ability, Texture2D>();
            abilityIcons[Slash] = Graphics.Slash;
            abilityIcons[Dash] = Graphics.Dash;
            abilityIcons[Lunge] = Graphics.Lunge;
            abilityIcons[Block] = Graphics.Shield;
            abilityIcons[FireSpore] = Graphics.SporeLogo;
        }

        public override void Update(Level level, float deltaTime) {
            flips = SpriteEffects.None;

            if(Input.IsPressed(Inputs.Right) && !Input.IsPressed(Inputs.Left)) {
                facing = Direction.Right;
            }
            else if(Input.IsPressed(Inputs.Left) && !Input.IsPressed(Inputs.Right)) {
                facing = Direction.Left;
                flips = SpriteEffects.FlipHorizontally;
            }
            else if(Input.IsPressed(Inputs.Up) && !Input.IsPressed(Inputs.Down)) {
                facing = Direction.Up;
                flips = SpriteEffects.FlipHorizontally;
            }
            else {
                facing = Direction.Down;
            }

            if(facing != wasFacing) {
                switch(facing) {
                    case Direction.Down:
                        currentAnimation = forward;
                        break;
                    case Direction.Up:
                        currentAnimation = back;
                        break;
                    case Direction.Right:
                    case Direction.Left:
                        currentAnimation = side;
                        break;
                }
            }

            currentAnimation.Update(deltaTime);
            wasFacing = facing; // detect change in animation next frame

            bool checkWalls = true;
            bool checkPits = true;

            switch(state) {
                case PlayerState.Normal:
                    float maxSpeed = 400;
                    switch(possessType) {
                        case EnemyTypes.Zombie:
                            maxSpeed = Zombie.MAX_SPEED;
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
                            cooldowns[1] = 0.5;
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
                        cooldowns[0] = 0.2;
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
                        cooldowns[1] = 1;
                        hitEnemies.Clear();
                    }
                    break;

                case PlayerState.Block:
                    timer -= deltaTime;
                    if(timer <= 0 || !Input.IsPressed(Inputs.Secondary)) {
                        // end block
                        state = PlayerState.Normal;
                        timer = 0;
                        cooldowns[1] = 2f;
                    }
                    break;
            }

            // check wall collision
            if(checkWalls) {
                CheckWallCollision(level, checkPits);
            }

            // abilities
            if(state == PlayerState.Normal) {
                if(cooldowns[0] <= 0 && Input.JustPressed(Inputs.Attack) && abilities[possessType][0] != null) {
                    abilities[possessType][0](level);
                }
                else if(cooldowns[1] <= 0 && Input.JustPressed(Inputs.Secondary) && abilities[possessType][1] != null) {
                    abilities[possessType][1](level);
                }
                else if(cooldowns[2] <= 0 && Input.JustPressed(Inputs.Tertiary) && abilities[possessType][2] != null) {
                    abilities[possessType][2](level);
                }
            }

            // possess enemies
            if(Input.JustPressed(Inputs.Possess)) {
                if(possessType == EnemyTypes.None && state == PlayerState.Normal) {
                    // find an enemy in possessing range
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
                        drawBox = possessTarget.DrawRect;
                        if(possessType == EnemyTypes.Mushroom) {
                            currentAnimation = Mushroom.Shoot;
                        }
                        for(int i = 0; i < 3; i++) {
                            cooldowns[i] = 0;
                        }
                    }
                }
                else if(possessType != EnemyTypes.None) {
                    // unpossess
                    health = ghostHealth;
                    possessType = EnemyTypes.None;
                    state = PlayerState.Normal;
                    invulnTime = 0.5f;
                    drawBox = playerDrawBox;
                    currentAnimation = forward;
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
            tint = Color.White;
            //sprite = Graphics.TempGhost;
            if(possessType == EnemyTypes.Zombie) {
                tint = Color.Brown;
            }
            else if(possessType == EnemyTypes.Mushroom) {
                //sprite = Graphics.Mushroom[0];
                if(state == PlayerState.Block) {
                    tint = Color.Pink;
                } else {
                    tint = Color.White;
                }
            }

            if(invulnTime > 0) {
                tint *= 0.5f;
            }
            sb.Draw(currentAnimation.CurrentSprite, DrawBox, null, tint, 0f, Vector2.Zero, flips, 1f);

            // draw attack
            switch (state) {
                case PlayerState.Lunge:
                case PlayerState.Slash:
                    if(attackArea != null) {
                        sb.Draw(Graphics.Slash, new Rectangle(attackArea.X + (int)Camera.Shift.X, attackArea.Y + (int)Camera.Shift.Y, attackArea.Width, attackArea.Height), Color.White);
                    }
                    break;
            }
            
        }

        public void DrawUI(SpriteBatch sb) {
            for(int i = 0; i < health; i++) {
                sb.Draw(Graphics.Pixel, new Rectangle(30 + i * 60, 30, 50, 50), possessType == EnemyTypes.None ? Color.Blue : Color.Red);
            }

            // draw ability buttons
            Vector2 buttonMid = new Vector2(1500, 100);
            int buttonLength = 50;
            int distFromMid = 50;
            Rectangle top = new Rectangle((int)buttonMid.X - buttonLength / 2, (int)buttonMid.Y - buttonLength / 2 - distFromMid, buttonLength, buttonLength);
            Rectangle bottom = new Rectangle((int)buttonMid.X - buttonLength / 2, (int)buttonMid.Y - buttonLength / 2 + distFromMid, buttonLength, buttonLength);
            Rectangle left = new Rectangle((int)buttonMid.X - buttonLength / 2 - distFromMid, (int)buttonMid.Y - buttonLength / 2, buttonLength, buttonLength);
            Rectangle right = new Rectangle((int)buttonMid.X - buttonLength / 2 + distFromMid, (int)buttonMid.Y - buttonLength / 2, buttonLength, buttonLength);
            sb.Draw(Graphics.Button, top, Color.White);
            sb.Draw(Graphics.Button, bottom, cooldowns[1] > 0 ? Color.Red : Color.Green); 
            sb.Draw(Graphics.Button, left, cooldowns[0] > 0 ? Color.Red : Color.Green);
            sb.Draw(Graphics.Button, right, cooldowns[2] > 0 ? Color.Red : Color.Green);

            int reducer = -7;
            if(abilities[possessType][0] != null) {
                left.Inflate(reducer, reducer);
                sb.Draw(abilityIcons[abilities[possessType][0]], left, Color.White);
            }
            if(abilities[possessType][1] != null) {
                bottom.Inflate(reducer, reducer);
                sb.Draw(abilityIcons[abilities[possessType][1]], bottom, Color.White); 
            }
            if(abilities[possessType][2] != null) {
                right.Inflate(reducer, reducer);
                sb.Draw(abilityIcons[abilities[possessType][2]], right, Color.White);
            }
        }

        public void TakeDamage(int damage) {
            if(invulnTime <= 0 && state != PlayerState.Block) {
                health -= damage;
                if(possessType == EnemyTypes.None) {
                    ghostHealth -= damage;
                }
                invulnTime = 2.0f;
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
            timer = 0.08f;
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
            level.Projectiles.Add(new Projectile(Mushroom.Spore, Midpoint, Input.GetAim(), true));
            cooldowns[0] = 0.75f;
            currentAnimation.Restart();
            level.Particles.Add(new Particle(Mushroom.SporeCloud, Midpoint - new Vector2(0, 25)));
        }

        // generates an attack area relative to the player. Uses the aim variable
        private void GenerateAttack(int length, int dist) {
            Vector2 rectMid = Midpoint + aim * dist;
            attackArea = new Rectangle((int)rectMid.X - length / 2, (int)rectMid.Y - length / 2, length, length);
        }
    }
}

﻿using Microsoft.Xna.Framework.Graphics;
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
        Slime,
        Blight
    }

    public enum PlayerState {
        Normal,
        Dash,
        Slash,
        Lunge,
        Block,
        Explode
    }

    public delegate void Ability(Level level);

    public class Player : Entity
    {
        public const int SELECT_DIST = 50; // distance from a dead enemy that the player can possess them
        public const int MAX_SPEED = 400;
        private const float DECAY_RATE = 5f;
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
        private float decayTimer; // tracks how long until the possessed body loses a health

        private readonly Dictionary<Ability, Texture2D> abilityIcons;

        public bool Possessing { get { return possessType != EnemyTypes.None; } } // whether or not the player is possessing an enemy

        public Player() : base(Vector2.Zero, 50, 50) {
            state = PlayerState.Normal;
            velocity = Vector2.Zero;
            hitEnemies = new List<Enemy>();
            health = 5;
            ghostHealth = 5;
            drawBox = playerDrawBox;

            sprite = null;
            currentAnimation = forward;

            cooldowns = new double[3];
            cooldowns[0] = 0.0f;
            cooldowns[1] = 0.0f;
            cooldowns[2] = 0.0f;

            // create enemy abilities
            abilities = new Dictionary<EnemyTypes, Ability[]>();
            abilities[EnemyTypes.None] = new Ability[3] { Slash, null, null };
            abilities[EnemyTypes.Zombie] = new Ability[3] { Slash, Lunge, null };
            abilities[EnemyTypes.Mushroom] = new Ability[3] { FireSpore, Block, null };
            abilities[EnemyTypes.Slime] = new Ability[3] { FireSlimes, DropPuddle, null };
            abilities[EnemyTypes.Blight] = new Ability[3] { Explode, null, null };

            // setup ability icons
            abilityIcons = new Dictionary<Ability, Texture2D>();
            abilityIcons[Slash] = Graphics.Slash;
            abilityIcons[Dash] = Graphics.Dash;
            abilityIcons[Lunge] = Graphics.Dash;
            abilityIcons[Block] = Graphics.Shield;
            abilityIcons[FireSpore] = Graphics.SporeLogo;
            abilityIcons[FireSlimes] = Graphics.SporeLogo;
            abilityIcons[DropPuddle] = Graphics.Drop;
            abilityIcons[Explode] = Graphics.Drop;
        }

        public override void Update(Level level, float deltaTime) {
            flips = SpriteEffects.None;

            if(possessType == EnemyTypes.None) {
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
            }

            currentAnimation.Update(deltaTime);
            wasFacing = facing; // detect change in animation next frame

            bool checkWalls = true;
            bool checkPits = true;

            switch(state) {
                case PlayerState.Normal:
                    Move(deltaTime, GetMaxSpeed());
                    break;

                case PlayerState.Explode:
                    Move(deltaTime, Blight.MAX_SPEED);

                    timer -= deltaTime;
                    if(timer <= 0) {
                        state = PlayerState.Normal;
                    }

                    Circle explosion = new Circle(Midpoint, Blight.EXPLOSION_RADIUS);
                    foreach(Enemy enemy in level.Enemies) {
                        if(enemy.HitCircle.Intersects(explosion) && !hitEnemies.Contains(enemy)) {
                            enemy.TakeDamage();
                            enemy.Push(aim * 1000);
                            hitEnemies.Add(enemy);
                        }
                    }
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
                    Move(deltaTime, GetMaxSpeed() / 2);
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
                        cooldowns[0] = 0.4;
                    }
                    break;

                case PlayerState.Lunge: // zombie dash
                    position += velocity * deltaTime;
                    timer -= deltaTime;
                    if(timer <= 0) {
                        timer = 0;
                        state = PlayerState.Normal;
                        cooldowns[1] = 1;
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

                        invulnTime = 0.5f;
                        decayTimer = DECAY_RATE;
                    }
                }
                else if(possessType != EnemyTypes.None) {
                    Unpossess();
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
            if(Possessing) {
                // lose health over time when possessing
                decayTimer -= deltaTime;
                if(decayTimer <= 0) {
                    health--;
                    decayTimer = DECAY_RATE;
                    if(health <= 0) {
                        Unpossess();
                    }
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            // draw attack
            switch (state) {
                case PlayerState.Slash:
                    if(attackArea != null) {
                        //sb.Draw(Graphics.Slash, new Rectangle(attackArea.X + (int)Camera.Shift.X, attackArea.Y + (int)Camera.Shift.Y, attackArea.Width, attackArea.Height), Color.White);
                        SpriteEffects flips = SpriteEffects.None;
                        Vector2 aim = Midpoint - attackArea.Center.ToVector2();
                        if(aim.X < 0) {
                            flips = SpriteEffects.FlipVertically;
                        }
                        float rotation = (float)Math.Atan2(aim.Y, aim.X);
                        Game1.RotateDraw(sb, Graphics.Slash, new Rectangle(attackArea.X + (int)Camera.Shift.X, attackArea.Y + (int)Camera.Shift.Y, attackArea.Width, attackArea.Height), Color.White, rotation, flips);
                    }
                    break;
            }

            // make sprite match the current enemy
            tint = Color.White;
            if(possessType == EnemyTypes.Zombie) {
                currentAnimation = new Animation(new Texture2D[]{Graphics.Zombie}, AnimationType.Hold, 1f);
            }
            else if(possessType == EnemyTypes.Slime) {
                currentAnimation = new Animation(new Texture2D[] { Graphics.Slime }, AnimationType.Hold, 1f);
            }
            else if(possessType == EnemyTypes.Mushroom) {
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

            if(state == PlayerState.Explode) {
                sb.Draw(Graphics.Button, new Rectangle((int)(Midpoint.X - Blight.EXPLOSION_RADIUS + Camera.Shift.X), (int)(Midpoint.Y - Blight.EXPLOSION_RADIUS + Camera.Shift.Y), Blight.EXPLOSION_RADIUS * 2, Blight.EXPLOSION_RADIUS * 2), Color.Orange);
            }
        }

        public void DrawUI(SpriteBatch sb) {
            // draw health
            int x = 0;
            for(int i = 0; i < ghostHealth; i++) {
                Rectangle drawZone = new Rectangle(30 + i * 60, 30, 50, 50);
                drawZone.Inflate(5, 5);
                sb.Draw(Graphics.Soul, drawZone, Color.White);
            }
            if(possessType != EnemyTypes.None) {
                for(int i = 0; i < health; i++) {
                    sb.Draw(Graphics.Heart, new Rectangle(30 + 60 * ghostHealth + i * 60, 30, 50, 50), Color.White);
                }
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

            top.Inflate(reducer, reducer);
            if(possessType == EnemyTypes.None) {
                sb.Draw(Graphics.Possess, top, Color.White);
            } else {
                sb.Draw(Graphics.Unpossess, top, Color.White);
            }
        }

        private void Unpossess() {
            health = ghostHealth;
            possessType = EnemyTypes.None;
            state = PlayerState.Normal;
            drawBox = playerDrawBox;
            currentAnimation = forward;
        }

        public void TakeDamage(int damage = 1) {
            if(invulnTime <= 0 && state != PlayerState.Block) {
                health -= damage;

                if(Possessing) {
                    invulnTime = 0.5f;
                } else {
                    ghostHealth -= damage;
                    invulnTime = 2f;
                }

                if(health <= 0) {
                    // die
                    if(Possessing) {
                        Unpossess();
                    } else {
                        // lose
                        Game1.Game.Lose();
                    }
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

        // determines movement speed based on enemy form
        private float GetMaxSpeed() {
            float maxSpeed = MAX_SPEED; // default player speed
            switch(possessType) {
                case EnemyTypes.Zombie:
                    maxSpeed = Zombie.MAX_SPEED + 50;
                    break;
                case EnemyTypes.Mushroom:
                    maxSpeed = 0;
                    break;
                case EnemyTypes.Slime:
                    maxSpeed = Slime.MAX_SPEED + 50;
                    break;
                case EnemyTypes.Blight:
                    maxSpeed = Blight.MAX_SPEED;
                    break;
            }
            return maxSpeed;
        }

        private void Dash(Level level) {
            state = PlayerState.Dash;
            timer = Zombie.LUNGE_DURATION;
            velocity = Input.GetAim() * Zombie.LUNGE_SPEED;
        }

        private void Slash(Level level) {
            state = PlayerState.Slash;
            timer = 0.2f;
            aim = Input.GetAim();
            hitEnemies.Clear();

            GenerateAttack(50, 50);
        }

        private void Lunge(Level level) {
            state = PlayerState.Lunge;
            timer = Zombie.LUNGE_DURATION;
            aim = Input.GetAim();
            velocity = aim * Zombie.LUNGE_SPEED;
            GenerateAttack(50, 50);
        }

        private void Block(Level level) {
            state = PlayerState.Block;
            timer = 2f; // max block time
        }

        private void FireSpore(Level level) {
            level.Projectiles.Add(new BounceSpore(Midpoint, Input.GetAim(), true));
            cooldowns[0] = 0.75f;
            currentAnimation.Restart();
            level.Particles.Add(new Particle(Mushroom.SporeCloud, Midpoint - new Vector2(0, 25)));
        }

        private void FireSlimes(Level level) {
            cooldowns[0] = 1f;
            level.Projectiles.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(1, 0), true));
            level.Projectiles.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(-1, 0), true));
            level.Projectiles.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(0, 1), true));
            level.Projectiles.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(0, -1), true));
        }

        private void DropPuddle(Level level) {
            cooldowns[1] = 4f;
            level.Projectiles.Add(new SlimePuddle(Midpoint, true));
        }

        private void Explode(Level level) {
            cooldowns[0] = 2f;
            state = PlayerState.Explode;
            timer = Blight.EXPLOSION_DURATION;
            hitEnemies.Clear();
        }

        // generates an attack area relative to the player. Uses the aim variable
        private void GenerateAttack(int length, int dist) {
            Vector2 rectMid = Midpoint + aim * dist;
            attackArea = new Rectangle((int)rectMid.X - length / 2, (int)rectMid.Y - length / 2, length, length);
        }

        // when player enters a room, walk up for a bit, returns distance travelled
        public float WalkIn(float deltaTime) {
            float distance = MAX_SPEED * deltaTime;
            position.Y -= distance;
            return distance;
        }
    }
}

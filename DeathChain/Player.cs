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
        Slime,
        Blight,
        Scarecrow,
        Beast
    }

    public enum PlayerState {
        Normal,
        Dash,
        Slash,
        Lunge,
        Block,
        Teleport
    }

    public delegate void Ability(Level level);

    public class Player : Entity
    {
        public const int SELECT_DIST = 100; // distance from a dead enemy that the player can possess them
        public const int MAX_SPEED = 400;
        private const float DECAY_RATE = 5f;
        private const float ACCEL = 10000.0f;
        private float friction = 2000f;
        private const int WIDTH = 50;

        private readonly Rectangle playerDrawBox = new Rectangle(0, -15, 50, 65);
        private readonly Animation forward = new Animation(Graphics.PlayerFront, AnimationType.Loop, 0.1f);
        private readonly Animation side = new Animation(Graphics.PlayerSide, AnimationType.Loop, 0.1f);
        private readonly Animation back = new Animation(Graphics.PlayerBack, AnimationType.Loop, 0.1f);

        private EnemyTypes possessType; // the type of enemy the player is controlling currently
        private PlayerState state;
        
        private int health;
        private int ghostHealth; // the ghost form keeps health even when forms change
        private float timer; // used for anything, each state uses it for up to one thing
        private float invulnTime; // after getting hit
        private double[] cooldowns; // cooldowns for the 3 abilities.
        private Dictionary<EnemyTypes, Ability[]> abilities;
        private SpriteEffects flips;
        private Vector2 selector; // used for abilities that select a spot on the level
        private float decayTimer; // tracks how long until the possessed body loses a health
        private Attack currentAttack;
        private bool reverseSlash;
        private float unpossessTimer;

        private readonly Dictionary<Ability, Texture2D> abilityIcons;

        public bool Possessing { get { return possessType != EnemyTypes.None; } } // whether or not the player is possessing an enemy
        public Vector2 Focus { get {
            if(state == PlayerState.Teleport) {
                return selector;
            } else {
                return Midpoint;
            }
        } }

        public Player() : base(Vector2.Zero, WIDTH, WIDTH) {
            state = PlayerState.Normal;
            velocity = Vector2.Zero;
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
            abilities[EnemyTypes.Scarecrow] = new Ability[3] { FlameBurst, Teleport, FlameSpiral };
            abilities[EnemyTypes.Beast] = new Ability[3] { BeastSlash, Lunge, null };

            // setup ability icons
            abilityIcons = new Dictionary<Ability, Texture2D>();
            abilityIcons[Slash] = Graphics.Slash;
            abilityIcons[BeastSlash] = Graphics.Slash;
            abilityIcons[Dash] = Graphics.Dash;
            abilityIcons[Lunge] = Graphics.Dash;
            abilityIcons[Block] = Graphics.Shield;
            abilityIcons[FireSpore] = Graphics.SporeLogo;
            abilityIcons[FireSlimes] = Graphics.SporeLogo;
            abilityIcons[DropPuddle] = Graphics.Drop;
            abilityIcons[Explode] = Graphics.ExplosionLogo;
            abilityIcons[Teleport] = Graphics.Dash;
            abilityIcons[FlameBurst] = Graphics.ExplosionLogo;
            abilityIcons[FlameSpiral] = Graphics.SporeLogo;
        }

        public override void Update(Level level, float deltaTime) {
            // choose correct animation
            if(possessType == EnemyTypes.None && state == PlayerState.Normal) {
                currentAnimation = forward;
                flips = SpriteEffects.None;

                
                if(Input.IsPressed(Inputs.Up)) { // first so right and left get override priority
                    currentAnimation = back;
                    flips = SpriteEffects.FlipHorizontally;
                }
                if(Input.IsPressed(Inputs.Right)) {
                    currentAnimation = side;
                    flips = SpriteEffects.None;
                }
                if(Input.IsPressed(Inputs.Left)) {
                    currentAnimation = side;
                    flips = SpriteEffects.FlipHorizontally;
                }
            }

            currentAnimation.Update(deltaTime);

            bool checkWalls = true;
            bool checkPits = true;

            switch(state) {
                case PlayerState.Normal:
                    Move(deltaTime, GetMaxSpeed());
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
                            cooldowns[1] = 0.5f;
                        }
                    }
                    break;

                case PlayerState.Slash:
                    Move(deltaTime, GetMaxSpeed() / 2);
                    currentAttack.Update(level, deltaTime);

                    if(!currentAttack.IsActive) {
                        state = PlayerState.Normal;
                        cooldowns[0] = 0.4;
                        currentAttack = null;
                        /*if(!Possessing) {
                            drawBox = playerDrawBox;
                        }*/
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
                        currentAnimation = new Animation(Mushroom.Hide, true);
                        currentAnimation.Next = new Animation(Mushroom.Shoot);
                    }
                    break;

                case PlayerState.Teleport:
                    // move the selector until the player lets go
                    Vector2 direction = selector - Midpoint;
                    if(direction != Vector2.Zero) {
                        direction.Normalize();
                    }
                    direction += Input.GetMoveDirection();
                    if(direction != Vector2.Zero) {
                        direction.Normalize();
                    }

                    selector += direction * 900 * deltaTime; // selector move speed

                    // keep selector in level
                    Rectangle bounds = level.Bounds;
                    bounds.Inflate(-Level.EDGE_BUFFER, -Level.EDGE_BUFFER);
                    if(selector.X < bounds.Left) {
                        selector.X = bounds.Left + 1; // add one so the point does not lie on top of any wall
                    }
                    if(selector.X > bounds.Right) {
                        selector.X = bounds.Right - 1;
                    }
                    if(!level.Cleared && selector.Y < bounds.Top) { // allow the player to leave the level when beaten
                        selector.Y = bounds.Top + 1;
                    }
                    if(selector.Y > bounds.Bottom) {
                        selector.Y = bounds.Bottom - 1;
                    }

                    bool overWall = false;
                    foreach(Wall wall in level.Walls) {
                        if(wall.Hitbox.Contains(selector)) {
                            overWall = true;
                            break;
                        }
                    }
                    if(!overWall && !Input.IsPressed(Inputs.Secondary)) {
                        state = PlayerState.Normal;
                        Midpoint = selector;
                        cooldowns[1] = 0.5f;
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
                    if(Possessing) {
                        if(health >= 6 && health < 10) {
                            health++;
                        }
                        else if(health >= 3) {
                            health += (possessTarget.MaxHealth <= 1 ? 1 : 2);
                        }
                        else {
                            health += possessTarget.MaxHealth;
                        }

                        if(health < possessTarget.MaxHealth) {
                            health = possessTarget.MaxHealth;
                        }
                    } else {
                        health = possessTarget.MaxHealth;
                        decayTimer = DECAY_RATE;
                    }
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
                }
            }

            // timers
            for(int i = 0; i < 3; i++) { // decrease cooldowns
                if(cooldowns[i] > 0) {
                    cooldowns[i] -= deltaTime;
                    if(cooldowns[i] < 0) {
                        cooldowns[i] = 0;
                    }
                }
            }

            if(invulnTime > 0) { // decrease invuln time
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

                if(Input.IsPressed(Inputs.Possess)) {
                    unpossessTimer += deltaTime;
                    if(unpossessTimer > 0.5f) {
                        Unpossess();
                        unpossessTimer = 0f;
                    }
                } else {
                    unpossessTimer = 0;
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            // draw attack
            if(currentAttack != null) {
                currentAttack.Draw(sb);
            }

            // draw selectors
            if(state == PlayerState.Teleport) {
                sb.Draw(Graphics.Scarecrow, new Rectangle((int)(selector.X + Camera.Shift.X - width / 2 + drawBox.X), (int)(selector.Y + Camera.Shift.Y - height / 2 + drawBox.Y), drawBox.Width, drawBox.Height), Color.Black);
            }

            // TEMPORARY: make sprite match the current enemy
            tint = Color.White;
            if(possessType == EnemyTypes.Zombie) {
                currentAnimation = new Animation(new Texture2D[]{Graphics.Zombie}, AnimationType.Hold, 1f);
                tint = Color.Gray;
            }
            else if(possessType == EnemyTypes.Slime) {
                currentAnimation = new Animation(new Texture2D[] { Graphics.Slime }, AnimationType.Hold, 1f);
                tint = Color.Gray;
            }
            else if(possessType == EnemyTypes.Scarecrow) {
                currentAnimation = new Animation(new Texture2D[] { Graphics.Scarecrow }, AnimationType.Hold, 1f);
                tint = Color.Gray;
            }
            else if(possessType == EnemyTypes.Blight) {
                currentAnimation = new Animation(new Texture2D[] { Graphics.Blight }, AnimationType.Hold, 1f);
                tint = Color.Gray;
            }
            else if(possessType == EnemyTypes.Beast) {
                currentAnimation = new Animation(new Texture2D[] { Graphics.Beast }, AnimationType.Hold, 1f);
                tint = Color.Gray;
            }

            if(invulnTime > 0) {
                tint *= 0.5f;
            }
            sb.Draw(currentAnimation.CurrentSprite, DrawBox, null, tint, 0f, Vector2.Zero, flips, 1f);
        }

        public void DrawUI(SpriteBatch sb) {
            // draw health
            for(int i = 0; i < ghostHealth; i++) {
                Rectangle drawZone = new Rectangle(30 + i * 60, 30, 50, 50);
                sb.Draw(Graphics.Soul, drawZone, Color.White);
            }
            if(Possessing) {
                for(int i = 0; i < health; i++) {
                    sb.Draw(Graphics.Heart, new Rectangle(30 + 60 * (ghostHealth + i), 30, 50, 50), Color.White);
                }

                // death clock
                sb.Draw(Graphics.DeathClock, new Rectangle(30, 100, 100, 100), Color.White);

                float angle = decayTimer / DECAY_RATE;
                angle *= 2f * (float)Math.PI;
                sb.Draw(Graphics.Pixel, new Rectangle(80, 150, 50, 5), null, Color.Black, (float)-Math.PI / 2f - angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);

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
            sb.Draw(Graphics.Possess, top, Color.White);
        }

        private void Unpossess() {
            health = ghostHealth;
            possessType = EnemyTypes.None;
            state = PlayerState.Normal;
            drawBox = playerDrawBox;
            width = WIDTH;
            height = WIDTH;
            currentAnimation = forward;
            cooldowns[0] = 0;
            currentAttack = null;
        }

        public void TakeDamage(Level level, int damage = 1) {
            if(invulnTime <= 0 && state == PlayerState.Block) {
                // don't take damage when blocking
                level.Particles.Add(new Particle(Mushroom.SporeCloud, Midpoint - new Vector2(0, 25)));
                invulnTime = 0.3f; // prevent rapid particles
                Input.Vibrate(0.4f, 0.1f);
                return;
            }

            if(invulnTime <= 0) {
                health -= damage;
                level.Particles.Add(new Particle(Mushroom.SporeCloud, Midpoint - new Vector2(0, 25)));

                if(Possessing) {
                    invulnTime = 1f;
                    Input.Vibrate(0.6f, 0.2f);
                } else {
                    ghostHealth -= damage;
                    invulnTime = 2f;
                    Input.Vibrate(1.0f, 0.2f);
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
                    maxSpeed = Zombie.MAX_SPEED + 100;
                    break;
                case EnemyTypes.Scarecrow:
                case EnemyTypes.Mushroom:
                    maxSpeed = 0;
                    break;
                case EnemyTypes.Slime:
                    maxSpeed = Slime.MAX_SPEED + 50;
                    break;
                case EnemyTypes.Blight:
                    maxSpeed = Blight.MAX_SPEED + 20;
                    break;
                case EnemyTypes.Beast:
                    maxSpeed = Beast.MAX_SPEED;
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
            reverseSlash = !reverseSlash;
            int dirMult = (reverseSlash ? 1 : -1);

            const float DURATION = 0.15f;
            currentAttack = new Attack(this, 50, Game1.RotateVector(Input.GetAim(), dirMult * (float)Math.PI / 6f), -dirMult * (float)Math.PI / 3f, DURATION, Graphics.SlashEffect, reverseSlash);

            // animate
            /*if(!Possessing) {
                currentAnimation = new Animation(Graphics.PlayerForwardSlash, AnimationType.Hold, DURATION / Graphics.PlayerForwardSlash.Length); 
                drawBox = new Rectangle(-20, -15, 90, 90);
                if(!reverseSlash) {
                    flips = SpriteEffects.FlipHorizontally;
                }
            }*/
        }

        private void BeastSlash(Level level) {
            state = PlayerState.Slash;
            reverseSlash = !reverseSlash;
            int dirMult = (reverseSlash ? 1 : -1);

            currentAttack = new Attack(this, Beast.ATTACK_SIZE, Game1.RotateVector(Input.GetAim(), dirMult * Beast.ATTACK_ANGLE / 2f), -dirMult * Beast.ATTACK_ANGLE, Beast.ATTACK_DURATION, Graphics.SlashEffect, reverseSlash);
        }

        private void Lunge(Level level) {
            state = PlayerState.Lunge;
            timer = Zombie.LUNGE_DURATION;
            velocity = Input.GetMoveDirection() * Zombie.LUNGE_SPEED;
        }

        private void Block(Level level) {
            state = PlayerState.Block;
            timer = 2f; // max block time
            currentAnimation = new Animation(Mushroom.Hide);
        }

        private void FireSpore(Level level) {
            level.Abilities.Add(new BounceSpore(Midpoint, Input.GetAim(), true));
            cooldowns[0] = 0.75f;
            currentAnimation.Restart();
            level.Particles.Add(new Particle(Mushroom.SporeCloud, Midpoint - new Vector2(0, 25)));
        }

        private void FireSlimes(Level level) {
            cooldowns[0] = 0.8f;
            level.Abilities.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(1, 0), true));
            level.Abilities.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(-1, 0), true));
            level.Abilities.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(0, 1), true));
            level.Abilities.Add(new Projectile(Slime.SLIMEBALL, Midpoint, new Vector2(0, -1), true));
        }

        private void DropPuddle(Level level) {
            cooldowns[1] = 4f;
            level.Abilities.Add(new Zone(Slime.SlimePuddle, Midpoint, true));
        }

        private void Explode(Level level) {
            cooldowns[0] = 1.2f;
            level.Abilities.Add(new Explosion(Midpoint, true, Blight.EXPLOSION_RADIUS, Blight.STARTUP, new Texture2D[] { Graphics.Button }));
        }

        private void Teleport(Level level) {
            state = PlayerState.Teleport;
            selector = Midpoint + Input.GetAim();
        }

        private void FlameBurst(Level level) {
            cooldowns[0] = 0.7f;
            level.Abilities.Add(new Explosion(Scarecrow.FlameBurst, Midpoint + Input.GetAim() * Scarecrow.BURST_RANGE, true));
        }

        private void FlameSpiral(Level level) {
            cooldowns[2] = 3f;
            level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2(1, 0), true));
            level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2((float)Math.Cos(2 * Math.PI / 3), (float)Math.Sin(2 * Math.PI / 3)), true));
            level.Abilities.Add(new SpiralFlame(Midpoint, new Vector2((float)Math.Cos(-2 * Math.PI / 3), (float)Math.Sin(-2 * Math.PI / 3)), true));
        }

        // when player enters a room, walk up for a bit, returns distance travelled
        public float WalkIn(float deltaTime) {
            float distance = MAX_SPEED * deltaTime;
            position.Y -= distance;
            return distance;
        }
    }
}

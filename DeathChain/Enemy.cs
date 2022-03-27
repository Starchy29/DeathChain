using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public abstract class Enemy : Entity
    {
        protected const int ACCEL = 2000;

        protected int health;
        private int maxHealth; // tells the player how much health to have when possessing this
        protected bool alive; // alive determines if the player can possess this, isActive determines if it should be deleted
        protected float timer; // for sub-classes to use however they want
        private float enemyTimer; // alive: red flash duration when hit, dead: despawn timer
        protected Vector2 direction; // the direction this moves towards, determined by sub classes
        protected int maxSpeed;
        protected float moveTimer; // for wandering, how long until this changes direction
        protected Attack attack; // melee attacks
        private SpriteEffects flips;

        private float attackTimer; 
        protected float startupDuration; // pause before attacking for reacting
        protected float cooldownDuration; // time after attacking when it can't attack anymore

        private EnemyTypes type;

        public EnemyTypes Type { get { return type; } }
        public bool Alive { get { return alive; } }
        public int MaxHealth { get { return maxHealth; } }
        public Rectangle DrawRect { get { return drawBox; } }

        public Enemy(EnemyTypes type, Vector2 midpoint, int width, int height, int health, int maxSpeed) : base(midpoint, width, height, Graphics.Pixel) {
            alive = true;
            this.health = health;
            this.type = type;
            this.maxSpeed = maxSpeed;
            maxHealth = health;
            startupDuration = 0.4f;
            cooldownDuration = 2f;
        }

        // subclasses use AliveUpdate() since they all behave the same when dead
        public sealed override void Update(Level level, float deltaTime) {
            if(currentAnimation != null) {
                currentAnimation.Update(deltaTime);
            }

            if(enemyTimer > 0) {
                enemyTimer -= deltaTime;
            }

            if(alive) {
                if(attackTimer > 0) {
                    // pause before attack
                    direction = Vector2.Zero;
                    attackTimer -= deltaTime;
                    if(attackTimer <= 0) {
                        AttackEffects(level);
                        attackTimer = -cooldownDuration;
                    }

                    CheckWallCollision(level, true);
                } else {
                    AliveUpdate(level, deltaTime); // changes direction variable

                    // check attack
                    if(attack != null) {
                        attack.Update(level, deltaTime);
                        if(!attack.IsActive) {
                            attack = null;
                        }
                    }

                    if(attackTimer < 0) {
                        attackTimer += deltaTime;
                        if(attackTimer >= 0) {
                            attackTimer = 0;
                        }
                    }
                }

                // move in target direction
                if(direction != Vector2.Zero) {
                    direction.Normalize(); 

                    // face move direction
                    if(direction.X > 0) {
                        flips = SpriteEffects.FlipHorizontally;
                    } else {
                        flips = SpriteEffects.None;
                    }
                }
                velocity += direction * ACCEL * deltaTime;
                ApplyFriction(deltaTime);

                // cap max speed unless moving backwards
                if(Vector2.Dot(direction, velocity) >= 0 && velocity.Length() > maxSpeed) {
                    velocity.Normalize();
                    velocity *= maxSpeed;
                }

                position += velocity * deltaTime;

                // check contact damage
                if(Hitbox.Intersects(Game1.Player.Hitbox)) {
                    Game1.Player.TakeDamage(level, 1);
                }
            } 
            else if(enemyTimer <= 0) { // death
                // decay body after some time
                IsActive = false;
            }
        }

        public override void Draw(SpriteBatch sb) {
            tint = Color.White;
            if(alive && enemyTimer > 0) {
                tint = Color.Red;
            }
            else if(!alive && Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= Player.SELECT_DIST) {
                tint = Color.LightBlue;
            }
            else if(!alive) {
                // temporary
                tint = Color.Black;
            }
            
            // copy of base.Draw() with flips
            if(sprite != null) {
                sb.Draw(sprite, DrawBox, null, tint, 0, Vector2.Zero, flips, 0f);
            } else {
                sb.Draw(currentAnimation.CurrentSprite, DrawBox, tint);
            }

            if(attack != null) {
                attack.Draw(sb);
            }
        }

        protected abstract void AliveUpdate(Level level, float deltaTime);

        protected virtual void AttackEffects(Level level) { } // probably all enemies should override this

        // enters the pause before attacking. Call this to attack
        protected void Attack() {
            attackTimer = startupDuration;
        }

        protected bool OffCooldown() {
            return attackTimer == 0;
        }

        public virtual void TakeDamage(Level level, int damage = 1) {
            health -= damage;
            enemyTimer = 0.1f; // red flash duration
            if(health <= 0) {
                // die
                alive = false;
                enemyTimer = 5; // time before body despawns;
                attack = null;
            }
        }

        // moves away from other enemies
        protected void Separate(Level level, float deltaTime) {
            foreach(Enemy enemy in level.Enemies) {
                if(enemy != this && enemy.alive && Vector2.Distance(Midpoint, enemy.Midpoint) <= 100) {
                    Vector2 moveAway = Midpoint - enemy.Midpoint;
                    velocity += moveAway * 15 * deltaTime;
                }
            }
        }

        // moves around walls
        protected void PassWalls(Level level) {
            Rectangle future = Hitbox;
            Vector2 playerSeek = Game1.Player.Midpoint - Midpoint;
            if(playerSeek != Vector2.Zero) {
                playerSeek.Normalize();
            }
            future.Offset(playerSeek * width);
            foreach(Wall wall in level.Walls) {
                if(wall.Hitbox.Intersects(future)) { // about to move into wall
                    // determine which direction to move around the wall, assume moving counter clockwise
                    direction = new Vector2(0, 0);
                    if(Hitbox.Bottom - 5 < wall.Hitbox.Top) { // above
                        direction += new Vector2(-1, 0); // addition allows diagonals at the corners
                    }
                    if(Hitbox.Top + 5 > wall.Hitbox.Bottom) { // below
                        direction += new Vector2(1, 0);
                    }
                    if(Hitbox.Left + 5 > wall.Hitbox.Right) { // right
                        direction += new Vector2(0, -1);
                    }
                    if(Hitbox.Right - 5 < wall.Hitbox.Left) { // left
                        direction += new Vector2(0, 1);
                    }

                    // check if should go clockwise
                    float playerAngle = Game1.GetVectorAngle(Game1.Player.Midpoint - wall.Midpoint);
                    float enemyAngle = Game1.GetVectorAngle(Midpoint - wall.Midpoint);

                    if(enemyAngle > Math.PI && playerAngle < Math.PI) {
                        // account for crossing 2PI / 0 boundary
                        enemyAngle -= 2 * (float)Math.PI;
                    }

                    if(enemyAngle < playerAngle && enemyAngle > playerAngle - (float)Math.PI) {
                        direction *= -1;
                    }
                    break;
                }
            }
        }

        protected void ChooseRandomDirection(List<Vector2> options = null) {
            moveTimer += 1f; // how often this changes direction

            if(options == null) {
                direction = new Vector2((float)Game1.RNG.NextDouble() - 0.5f, (float)Game1.RNG.NextDouble() - 0.5f);
            } else {
                direction = options[Game1.RNG.Next(options.Count)];
            }
        }

        protected void ApplyFriction(float deltaTime, float amount = 1000) {
            Vector2 direction = -velocity;
            if(direction != Vector2.Zero) {
                direction.Normalize();
                velocity += direction * deltaTime * amount;
                if(Vector2.Dot(direction, velocity) > 0) {
                    // started moving backwards: stop instead
                    velocity = Vector2.Zero;
                }
            }
        }
    }
}

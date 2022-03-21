using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public class Zombie : Enemy
    {
        public const int MAX_SPEED = 150;
        public const float LUNGE_DURATION = 0.1f;
        public const int LUNGE_SPEED = 1500;

        private const float ATTACK_COOLDOWN = 2f;

        private bool lunging; // only 2 states

        public Zombie(int x, int y) : base(EnemyTypes.Zombie, new Vector2(x, y), 50, 50, 3, MAX_SPEED) {
            sprite = Graphics.Zombie;
            lunging = false;
            drawBox.Inflate(20, 16);
            drawBox.Offset(0, -15);
        }

        protected override void AliveUpdate(Level level, float deltaTime) {
            if(lunging) {
                if(timer < 0) {
                    // pausing before attack
                    timer += deltaTime;
                    if(timer >= 0) {
                        // choose an attack
                        if(Game1.RNG.NextDouble() <= 0.4) { // 40% chance to lunge
                            // lunge
                            timer = LUNGE_DURATION;
                            direction = Game1.Player.Midpoint - Midpoint;
                            if(direction != Vector2.Zero) {
                                direction.Normalize();
                            }
                            velocity = direction * LUNGE_SPEED;
                            maxSpeed = LUNGE_SPEED;
                        } else {
                            // slash
                            lunging = false;
                            timer = ATTACK_COOLDOWN;
                            Vector2 aim = Game1.Player.Midpoint - Midpoint;
                            attack = new Attack(this, 50, Game1.RotateVector(aim, -(float)Math.PI / 6f), (float)Math.PI / 3f, 0.2f, Graphics.SlashEffect);
                        }
                    }
                }
                else {
                    // lunging
                    timer -= deltaTime;
                    if(timer <= 0) {
                        // end lunge
                        timer = ATTACK_COOLDOWN; // don't lunge again for at least this time
                        lunging = false;
                        velocity = Vector2.Zero;
                        maxSpeed = MAX_SPEED;
                    }
                }
            } else {
                // move towards player
                direction = Game1.Player.Midpoint - Midpoint;

                PassWalls(level);
                Separate(level, deltaTime); // move away from other enemies

                // chance to lunge when close enough
                if(timer > 0) {
                    timer -= deltaTime;
                }

                if(timer <= 0) {
                    timer += 0.4f; // how often it checks whether or not to attack
                    if(Vector2.Distance(Game1.Player.Midpoint, Midpoint) <= 180) { // attack player if close enough
                        if(Game1.RNG.NextDouble() <= 0.7) {
                            // begin attack
                            timer = -0.4f; // pause time at start of attack
                            velocity = Vector2.Zero;
                            direction = Vector2.Zero;
                            lunging = true;
                        }
                    }
                }
            }

            CheckWallCollision(level, true);
        }
    }
}

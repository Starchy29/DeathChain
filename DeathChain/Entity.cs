using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    public enum Direction {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public abstract class Entity
    {
        protected Vector2 position;
        private Vector2 lastPos;
        protected int width;
        protected int height;
        protected Rectangle hitBox; // relative to position in local space
        protected Texture2D sprite;
        protected bool active;

        public bool IsActive { get { return active; } }
        public Rectangle Hitbox { get { return new Rectangle((int)(position.X + hitBox.X), (int)(position.Y + hitBox.Y), hitBox.Width, hitBox.Height); } } // transferred to global space
        public Vector2 Midpoint { get { return new Vector2(position.X + width / 2, position.Y + height / 2); } }
        private Rectangle DrawBox { get { return new Rectangle((int)(Camera.Shift.X + position.X), (int)(Camera.Shift.Y + position.Y), width, height); } }

        public Entity(int x, int y, int width, int height, Texture2D sprite = null) {
            position = new Vector2(x, y);
            lastPos = position;
            this.width = width;
            this.height = height;
            hitBox = new Rectangle(0, 0, width, height); // default hitbox lines up with visual box exactly
            this.sprite = sprite;
            active = true;
        }

        public virtual void Update(Level level, float deltaTime) {}

        public virtual void Draw(SpriteBatch sb) {
            if(sprite != null) {
                sb.Draw(sprite, DrawBox, Color.White);
            }
        }

        protected bool Collides(Entity other) {
            return this.Hitbox.Intersects(other.Hitbox);
        }

        // a function that allows entities to easily check for wall collision. May done manually if necessary
        protected List<Direction> CheckWallCollision(Level level, bool checkPits) {
            List<Direction> collisionDirections = new List<Direction>();
            foreach(Wall wall in level.Walls) {
                if(checkPits || !wall.IsPit) {
                    if(Hitbox.Intersects(wall.Hitbox)) {
                        Direction pushDirection = Direction.None;
                        // move outside of wall by checking which side this entered from
                        if(lastPos.X + width <= wall.position.X) {
                            pushDirection = Direction.Left;
                        }
                        else if(lastPos.X >= wall.position.X + wall.width) {
                            pushDirection = Direction.Right;
                        }
                        else if(lastPos.Y + height <= wall.position.Y) {
                            pushDirection = Direction.Up;
                        }
                        else if(lastPos.Y >= wall.position.Y + wall.height) {
                            pushDirection = Direction.Down;
                        }
                        else {
                            // was in the wall last frame: push to closest edge instead
                            Vector2 pushAngle = Midpoint - wall.Midpoint;
                            pushDirection = Direction.Up;
                            if(pushAngle.Y > 0) {
                                pushDirection = Direction.Down;
                            }
                            if(Math.Abs(pushAngle.X) > Math.Abs(pushAngle.Y)) {
                                if(pushAngle.X > 0) {
                                    pushDirection = Direction.Right;
                                } else {
                                    pushDirection = Direction.Left;
                                }
                            }
                        }

                        switch(pushDirection) {
                            case Direction.Up:
                                position.Y = wall.position.Y - height;
                                break;
                            case Direction.Down:
                                position.Y = wall.position.Y + wall.height;
                                break;
                            case Direction.Left:
                                position.X = wall.position.X - width;
                                break;
                            case Direction.Right:
                                position.X = wall.position.X + wall.width;
                                break;
                        }

                        if(pushDirection != Direction.None) {
                            collisionDirections.Add(pushDirection);
                        }
                    }
                }
            }

            lastPos = position;
            return collisionDirections;
        }
    }
}

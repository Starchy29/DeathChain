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
        protected int width;
        protected int height;
        protected Rectangle drawBox; // relative to position in local space
        protected Texture2D sprite;
        protected Color tint;
        //protected Direction facing;
        protected Vector2 velocity;

        public bool IsActive { get; set; }
        public Rectangle Hitbox { get { return new Rectangle((int)position.X, (int)position.Y, width, height); } }
        public Vector2 Midpoint { get { return new Vector2(position.X + width / 2, position.Y + height / 2); } }
        public Vector2 Position { get { return position; } }
        public Vector2 Velocity { get { return velocity; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        protected Rectangle DrawBox { get { return new Rectangle((int)(Camera.Shift.X + position.X + drawBox.X), (int)(Camera.Shift.Y + position.Y + drawBox.Y), drawBox.Width, drawBox.Height); } }

        public Entity(int x, int y, int width, int height, Texture2D sprite = null) {
            position = new Vector2(x, y);
            this.width = width;
            this.height = height;
            drawBox = new Rectangle(0, 0, width, height); // default visual box lines up with drawbox box exactly
            this.sprite = sprite;
            IsActive = true;
            tint = Color.White;
            //facing = Direction.Down;
        }

        public virtual void Update(Level level, float deltaTime) {}

        public virtual void Draw(SpriteBatch sb) {
            if(sprite != null) {
                sb.Draw(sprite, DrawBox, tint);
            }
        }

        protected bool Collides(Entity other) {
            return this.Hitbox.Intersects(other.Hitbox);
        }

        public float DistanceTo(Entity other) {
            return Vector2.Distance(Midpoint, other.Midpoint);
        }

        public virtual void Push(Vector2 force) {
            velocity += force;
        }

        // a function that allows entities to easily check for wall collision. May done manually if necessary
        protected List<Direction> CheckWallCollision(Level level, bool checkPits) {
            List<Direction> collisionDirections = new List<Direction>();
            foreach(Wall wall in level.Walls) {
                if(checkPits || !wall.IsPit) {
                    if(Collides(wall)) {
                        Direction pushDirection = Direction.None;

                        Vector2 pushAngle = Midpoint - wall.Midpoint;
                        pushAngle.X /= wall.width;
                        pushAngle.Y /= wall.height;
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

            return collisionDirections;
        }
    }
}

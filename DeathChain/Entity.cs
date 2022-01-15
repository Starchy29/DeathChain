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
        protected Vector2 velocity;
        protected Animation currentAnimation;

        public bool IsActive { get; set; }
        public Rectangle Hitbox { get { return new Rectangle((int)position.X, (int)position.Y, width, height); } }
        public Circle HitCircle { get { return new Circle(Midpoint, (width > height ? width : height) / 2f); } }
        public Vector2 Midpoint { 
            get { return new Vector2(position.X + width / 2, position.Y + height / 2); }
            set { position = new Vector2(value.X - width / 2, value.Y - width / 2); }
        }
        public Vector2 Position { get { return position; } }
        public Vector2 Velocity { get { return velocity; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }

        protected Rectangle DrawBox { get { return new Rectangle((int)(Camera.Shift.X + position.X + drawBox.X), (int)(Camera.Shift.Y + position.Y + drawBox.Y), drawBox.Width, drawBox.Height); } }

        public Entity(Vector2 midpoint, int width, int height, Texture2D sprite = null) {
            position = new Vector2(midpoint.X - width / 2, midpoint.Y - height / 2);
            this.width = width;
            this.height = height;
            drawBox = new Rectangle(0, 0, width, height); // default visual box lines up with drawbox box exactly
            this.sprite = sprite;
            IsActive = true;
            tint = Color.White;
        }

        public virtual void Update(Level level, float deltaTime) {}

        // each entity uses a single image or animations
        public virtual void Draw(SpriteBatch sb) {
            if(sprite != null) {
                sb.Draw(sprite, DrawBox, tint);
            } else {
                sb.Draw(currentAnimation.CurrentSprite, DrawBox, tint);
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

        //  Will stop the velocity if necessary. Returns a list that says which sides of this entity collided
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
                                velocity.Y = 0;
                                position.Y = wall.position.Y - height;
                                collisionDirections.Add(Direction.Down);
                                break;
                            case Direction.Down:
                                velocity.Y = 0;
                                position.Y = wall.position.Y + wall.height;
                                collisionDirections.Add(Direction.Up);
                                break;
                            case Direction.Left:
                                velocity.X = 0;
                                position.X = wall.position.X - width;
                                collisionDirections.Add(Direction.Right);
                                break;
                            case Direction.Right:
                                velocity.X = 0;
                                position.X = wall.position.X + wall.width;
                                collisionDirections.Add(Direction.Left);
                                break;
                        }
                    }
                }
            }

            return collisionDirections;
        }
    }
}

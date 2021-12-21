using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    abstract class Entity
    {
        protected Vector2 position;
        protected int width;
        protected int height;
        protected Rectangle hitBox; // relative to position in local space
        protected Texture2D sprite;

        public Rectangle Hitbox { get { return new Rectangle((int)(position.X + hitBox.X), (int)(position.Y + hitBox.Y), hitBox.Width, hitBox.Height); } } // transferred to global space
        private Rectangle DrawBox { get { return new Rectangle(); } }

        public Entity(int x, int y, int width, int height, Texture2D sprite = null) {
            position = new Vector2(x, y);
            this.width = width;
            this.height = height;
            hitBox = new Rectangle(0, 0, width, height); // default hitbox lines up with visual box exactly
            this.sprite = sprite;
        }

        public virtual void Update(Level level) {}

        public virtual void Draw(SpriteBatch sb) {
            sb.Draw(sprite, DrawBox, Color.White);
        }

        protected bool Collides(Entity other) {
            return this.Hitbox.Intersects(other.Hitbox);
        }
    }
}

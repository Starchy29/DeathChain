using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeathChain
{
    // An animation with a position and no game logic. These are removed when completed
    public class Particle
    {
        private Rectangle area;
        private Animation animation;
        private float timer;
        private readonly float duration;
        private float rotation;

        public bool Done { get { return timer >= duration; } }

        public Particle(Rectangle area, Texture2D[] sprites, float duration) {
            this.area = area;
            this.duration = duration;
            rotation = 0f;
            timer = 0;

            animation = new Animation(sprites, AnimationType.Hold, duration / sprites.Length); // animation is automatically normal type and divided among duration
        }

        // copy from another particle, but reposition
        public Particle(Particle other, Vector2 midpoint, float rotation = 0f) {
            this.area = other.area;
            area.X = (int)midpoint.X - area.Width / 2;
            area.Y = (int)midpoint.Y - area.Height / 2;
            this.duration = other.duration;
            this.animation = other.animation; // copies because struct
            this.rotation = rotation;
            timer = 0;
        }

        public void Update(float deltaTime) {
            timer += deltaTime;
            animation.Update(deltaTime);
        }

        public void Draw(SpriteBatch sb) {
            area.Offset(Camera.Shift.X, Camera.Shift.Y);

            Game1.RotateDraw(sb, animation.CurrentSprite, area, Color.White, rotation);
        }
    }
}

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
    class Particle
    {
        private Rectangle area;
        private Animation animation;
        private float timer;
        private readonly float duration;

        public bool Done { get { return timer >= duration; } }

        public Particle(Rectangle area, Texture2D[] sprites, float duration) {
            this.area = area;
            this.duration = duration;
            timer = 0;

            animation = new Animation(sprites, AnimationType.Hold, duration / sprites.Length); // animation is automatically normal type and divided among duration
        }

        public void Update(float deltaTime) {
            timer += deltaTime;
            animation.Update(deltaTime);
        }

        public void Draw(SpriteBatch sb) {
            sb.Draw(animation.CurrentSprite, area, Color.White);
        }
    }
}

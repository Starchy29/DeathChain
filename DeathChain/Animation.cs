using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeathChain
{
    public enum AnimationType {
        Hold, // regular animation type, keeps using the last frame when ended
        Reverse, // same as hold, but runs the sprite array backwards
        Loop,
        Oscillate, // loops back and forth
    }

    // Animations will be defined as class members, then auto-copied when assigned since they are a struct. Do not use StepSprite() on the original.
    struct Animation
    {
        private Texture2D[] sprites;
        private AnimationType type;
        private readonly float secondsPerFrame; // how fast the sprites change
        private float timer;
        private bool backwards;
        private int frame;

        public Texture2D CurrentSprite { get { return sprites[frame]; } }

        // Define an animation that will be copied from. Sprite array must contain at least one element
        public Animation(Texture2D[] sprites, AnimationType type, float secondsPerFrame) {
            this.sprites = sprites;
            this.type = type;
            this.secondsPerFrame = secondsPerFrame;
            backwards = false;
            timer = 0;
            frame = 0;
            if(type == AnimationType.Reverse) {
                // when reversed, start on last frame
                frame = sprites.Length - 1;
                backwards = true; // not necessary, but accurate
            }
        }

        // move the animation forward the indicated amount
        public void Update(float deltaTime) {
            timer += deltaTime;
            while(timer >= secondsPerFrame) {
                timer -= secondsPerFrame;

                // step the animation a frame
                switch(type) {
                    case AnimationType.Hold:
                        if(frame < sprites.Length - 1) {
                            frame++;
                        }
                        break;
                    case AnimationType.Reverse:
                        if(frame > 0) {
                            frame--;
                        }
                        break;
                    case AnimationType.Loop:
                        frame++;
                        if(frame >= sprites.Length) {
                            // restart animation
                            frame = 0;
                        }
                        break;
                    case AnimationType.Oscillate:
                        if(backwards) {
                            frame--;
                            if(frame < 0) {
                                // go forwards
                                backwards = false;
                                if(sprites.Length >= 2) {
                                    frame = 1;
                                }
                            }
                        } else {
                            frame++;
                            if(frame >= sprites.Length) {
                                // go backwards
                                backwards = true;
                                if(sprites.Length >= 2) {
                                    frame = sprites.Length - 2; // go back 1 frame instead of forward 1
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}

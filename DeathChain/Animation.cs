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
        Rebound, // plays forward, then backwards, then holds the first frame
        Loop, // restarts when reaches the end
        Oscillate, // rebound that loops forever
    }

    // Animations will be defined as class members, then auto-copied when assigned since they are a struct. Do not update on the original.
    public struct Animation
    {
        private Texture2D[] sprites;
        private AnimationType type;
        private readonly float secondsPerFrame; // how fast the sprites change
        private float timer;
        private bool backwards;
        private int frame;

        public Texture2D CurrentSprite { get { return sprites[frame]; } }

        // Define an animation that will be copied from. Sprite array must contain at least one element
        public Animation(Texture2D[] sprites, AnimationType type, float secondsPerFrame, bool startAtEnd = false) {
            this.sprites = sprites;
            this.type = type;
            this.secondsPerFrame = secondsPerFrame;
            backwards = false;
            timer = 0;
            frame = 0;
            if(type == AnimationType.Reverse) {
                // when reversed, start on last frame
                frame = sprites.Length - 1;
                backwards = true;
            }

            if(startAtEnd) {
                switch(type) {
                    case AnimationType.Hold:
                        frame = sprites.Length - 1;
                        break;
                    case AnimationType.Reverse:
                    case AnimationType.Rebound:
                        backwards = true;
                        frame = 0;
                        break;
                }
            }
        }

        // move the animation forward the indicated amount
        public void Update(float deltaTime) {
            timer += deltaTime;
            while(timer >= secondsPerFrame) {
                timer -= secondsPerFrame;

                // step the animation a frame
                if(backwards) {
                    frame--;

                    // check if reached the beginning
                    if(frame < 0) {
                        switch(type) {
                            case AnimationType.Reverse:
                            case AnimationType.Rebound:
                                frame++; // stay on first frame
                                break;
                            case AnimationType.Oscillate:
                                backwards = false;
                                frame += 2; // go forward one instead of back one
                                break;

                            // hold and loop never moves backwards
                        }
                    }
                } else { // forwards
                    frame++;

                    // check if reached the end
                    if(frame >= sprites.Length) {
                        switch(type) {
                            case AnimationType.Hold:
                                frame--; // stay on last frame
                                break;
                            case AnimationType.Rebound:
                            case AnimationType.Oscillate:
                                backwards = true;
                                frame -= 2; // go back one instead of forward one
                                break;
                            case AnimationType.Loop:
                                frame = 0; // start at beginning
                                break;

                            // reverse never moves forward
                        }
                    }
                }
            }
        }

        // does nothing for loop and oscillate
        public void Restart() {
            switch(type) {
                case AnimationType.Hold:
                    frame = 0;
                    break;
                case AnimationType.Rebound:
                    frame = 0;
                    backwards = false;
                    break;
                case AnimationType.Reverse:
                    frame = sprites.Length - 1;
                    break;
            }
        }
    }
}

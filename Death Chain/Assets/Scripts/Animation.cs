using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType {
    Forward, // play normally, then hold the last frame
    Reverse, // play back to front then stop
    Loop, // go back to beginning when done
    Oscillate, // reverse direction when reaching the beginning or end
}

// defines an animation. The object's script must update this every frame by passing in its SpriteRenderer to allow this to change the sprite
public class Animation
{
    private Sprite[] sprites;
    private AnimationType type;
    private float frameTime; // time spent on each frame

    private float timer;
    private int frame; // the current frame of the animation, an index of the sprites array
    private bool reverse; // false: moving forwards

    // duration is the time spent from one end of the sprites array to the other
    public Animation(Sprite[] sprites, AnimationType type, float duration) {
        this.sprites = sprites;
        this.type = type;
        frameTime = duration / sprites.Length;

        Reset();
    }

    // starts the animation over from the beginning
    public void Reset() {
        timer = frameTime;

        frame = 0;
        reverse = false;

        if(type == AnimationType.Reverse) {
            frame = sprites.Length - 1;
            reverse = true;
        }
    }

    // keeps the stats of the animation, but changes how it animates
    public void ChangeType(AnimationType newType) {
        type = newType;
        Reset();
    }

    // called by the game object using this animation. It passes in its own sprite renderer
    public void Update(SpriteRenderer animationTarget) {
        timer -= Time.deltaTime;
        if(timer <= 0) {
            timer += frameTime;

            // go to the next frame
            if(reverse) {
                frame--;

                if(frame < 0) {
                    switch(type) {
                        case AnimationType.Reverse:
                            frame = 0; // stay on first frame
                            break;

                        case AnimationType.Oscillate:
                            frame = 1; // start forwards
                            reverse = false;
                            break;
                    }
                }
            } else {
                frame++;

                if(frame > sprites.Length - 1) {
                    switch(type) {
                        case AnimationType.Forward:
                            frame = sprites.Length - 1; // stay on last frame
                            break;

                        case AnimationType.Loop:
                            frame = 0;
                            break;

                        case AnimationType.Oscillate:
                            frame = sprites.Length - 2; // start backwards
                            reverse = true;
                            break;
                    }
                }
            }

            // set frame
            animationTarget.sprite = sprites[frame];
        }
    }


}

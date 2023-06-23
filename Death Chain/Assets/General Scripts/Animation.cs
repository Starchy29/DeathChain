using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType {
    Forward, // play normally, then hold the last frame
    Reverse, // play back to front then stop
    Rebound, // out and back
    Loop, // go back to beginning when done
    Oscillate, // reverse direction when reaching the beginning or end, keeps going
}

public delegate void Event();

// defines an animation. The object's script must update this every frame by passing in its SpriteRenderer to allow this to change the sprite
public class Animation
{
    private Sprite[] sprites;
    private AnimationType type;
    private float frameTime; // time spent on each frame
    private float duration;

    private float timer;
    private int frame; // the current frame of the animation, an index of the sprites array
    private bool reverse; // false: moving forwards
    private float pauseTime;

    public bool Done { get; private set; } // tells other classes when this animation has finished
    public Event OnComplete { get; set; }
    public float Duration { get { return duration; } }

    // duration is the time spent from one end of the sprites array to the other
    public Animation(Sprite[] sprites, AnimationType type, float duration) {
        this.duration = duration;
        this.sprites = sprites;
        this.type = type;
        frameTime = duration / sprites.Length;

        Reset();
    }

    // starts the animation over from the beginning
    public void Reset() {
        timer = 0; // set to first frame immediately
        frame = -1;
        Done = false;
        pauseTime = 0;

        reverse = false;

        if(type == AnimationType.Reverse) {
            frame = sprites.Length - 2;
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
        if(pauseTime > 0) {
            pauseTime -= Time.deltaTime;
            return;
        }

        timer -= Time.deltaTime;
        if(timer <= 0) {
            timer += frameTime;

            // go to the next frame
            if(reverse) {
                frame--;

                if(frame < 0) {
                    switch(type) {
                        case AnimationType.Reverse:
                        case AnimationType.Rebound:
                            frame = 0; // stay on first frame
                            Done = true;
                            if(OnComplete != null) {
                                OnComplete();
                            }
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
                            Done = true;
                            if(OnComplete != null) {
                                OnComplete();
                            }
                            break;

                        case AnimationType.Loop:
                            frame = 0;
                            break;

                        case AnimationType.Rebound:
                        case AnimationType.Oscillate:
                            frame = sprites.Length - 2; // start backwards
                            reverse = true;
                            if(OnComplete != null) {
                                OnComplete();
                            }
                            break;
                    }
                }
            }

            // set frame
            animationTarget.sprite = sprites[frame];
        }
    }

    public void AddPause(float duration) {
        pauseTime = duration;
    }
}

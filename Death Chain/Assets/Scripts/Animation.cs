using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType {
    Forward, // play normally, then hold the last frame
    Reverse, // play back to front then stop
    Loop, // go back to beginning when done
    Oscillate, // reverse direction when reaching the beginning or end
}

// must be attached to a game object that has a SpriteRenderer component. Changes the sprite of that component
public class Animation : MonoBehaviour
{
    [SerializeField] private Texture2D[] sprites; // the sequence of sprites, set in the inspector
    [SerializeField] private AnimationType type;
    [SerializeField] private float duration; // time spent going from side of the array to the other

    private SpriteRenderer renderer;
    private float timer;
    private float frame;
    private float frameTime; // time spent on each frame
    private bool reverse; // false: moving forwards

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>();
        frameTime = duration / sprites.Length;
        timer = frameTime;

        frame = 0;
        reverse = false;

        if(type == AnimationType.Reverse) {
            frame = sprites.Length - 1;
            reverse = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
        }
    }
}

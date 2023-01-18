using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a game object that is simply an animation that deletes itself when done
public class Particle : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float duration;
    private Animation effect;

    void Start()
    {
        effect = new Animation(sprites, AnimationType.Forward, duration);
    }

    void Update()
    {
        effect.Update(GetComponent<SpriteRenderer>());
        if(effect.Done) {
            Destroy(gameObject);
        }
    }
}

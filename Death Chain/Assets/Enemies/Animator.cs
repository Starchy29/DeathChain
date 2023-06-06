using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a game object that is simply an animation that deletes itself when done
public class Animator : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private AnimationType type;
    [SerializeField] private float duration;
    [SerializeField] private bool destroyWhenDone;
    private Animation effect;

    void Start()
    {
        effect = new Animation(sprites, type, duration);
    }

    void Update()
    {
        effect.Update(GetComponent<SpriteRenderer>());
        if(destroyWhenDone && effect.Done) {
            Destroy(gameObject);
        }
    }
}

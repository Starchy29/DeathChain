using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private bool dead; // enemies lie on the ground for some time when dead so the player can possess them
    protected bool ally = false; // whether or not this is fighting for the player
    protected bool unmoving = false; // true means this enemy does not move and cannot receive knockback

    protected float maxSpeed;
    protected Controller controller;

    private Rigidbody2D body;
    
    // how is health handled?

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        ChildStart();
    }
    protected abstract void ChildStart();

    // Update is called once per frame
    void Update()
    {
        controller.Update(gameObject);

        // apply friction
        const float FRICTION = 20;
        if (body.velocity != Vector2.zero) {
            Vector2 friction = -body.velocity.normalized * Time.deltaTime * FRICTION;
            body.velocity += friction;
            
            // check if friction made this start moving backwards
            if(Vector2.Dot(body.velocity, friction) > 0) {
                body.velocity = Vector2.zero;
            }
        }

        // movement
        const float ACCEL = 80;
        Vector2 moveDirection = controller.GetMoveDirection();
        if(moveDirection != Vector2.zero) {
            body.velocity += moveDirection * Time.deltaTime * ACCEL;
            
            // cap speed
            if(body.velocity.sqrMagnitude > maxSpeed * maxSpeed) {
                body.velocity = body.velocity.normalized;
                body.velocity *= maxSpeed;
            }
        }

        // figure out knockback

        // abilities handled in each sub class
        UpdateAbilities();
    }

    public abstract void UpdateAbilities();

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.);
    }
}

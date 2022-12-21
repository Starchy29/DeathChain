using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private bool dead; // enemies lie on the ground for some time when dead so the player can possess them
    protected bool isAlly = false; // whether or not this is fighting for the player
    protected bool sturdy = false; // true means this enemy cannot receive knockback

    protected float maxSpeed; // how fast this character can move without factoring in status effects. Can be changed by own abilities
    protected Controller controller;

    private Rigidbody2D body;
    private Statuses statuses = new Statuses(); // conveniently track all status effects
    protected int health;

    private float poisonTimer; // tracks when to deal poison damage
    private bool knocked = false; // true means movement is locked as this is being pushed

    public float DamageMultiplier { get { 
            return 1 + (statuses.HasStatus(Status.Strength) ? 0.5f : 0) - (statuses.HasStatus(Status.Weakness) ? 0.5f : 0); } }
    public bool IsAlly { get { return isAlly; } }
    public bool IsPlayer { get { return controller is PlayerController; } }

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
        statuses.Update();

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
        
        if(!knocked) {
            // regular movement
            float currentMaxSpeed = maxSpeed;
            if(statuses.HasStatus(Status.Freeze)) {
                currentMaxSpeed = 0;
            } else {
                currentMaxSpeed *= (statuses.HasStatus(Status.Speed) ? 1.5f : 1) * (statuses.HasStatus(Status.Slow) ? 0.5f : 1);
            }
            if(maxSpeed > 0) {
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
            }
        }
        else if(body.velocity == Vector2.zero) { // check for end of knockback
            knocked = false;
        }

        // abilities handled in each sub class
        UpdateAbilities();

        // manage poison damage
        if(statuses.HasStatus(Status.Poison)) {
            poisonTimer -= Time.deltaTime;
            if(poisonTimer <= 0) {
                poisonTimer += 0.5f; // poison tick rate
                TakeDamage(1); // damage per tick
            }
        }
    }

    protected abstract void UpdateAbilities();

    public void TakeDamage(int amount) {
        if(statuses.HasStatus(Status.Vulnerability)) {
            amount *= 2;
        }
        if(statuses.HasStatus(Status.Resistance)) {
            amount /= 2;
        }

        health -= amount;
        Debug.Log(health);

        // check for death
        if(health <= 0) {

        }
    }

    public void Push(Vector2 force) {
        knocked = true;
        body.velocity += force;
    }

    // apply a status effect for some time. If no time parameter is given, it is set to an hour to represent infinite duration
    public void ApplyStatus(Status effect, float duration = 60 * 60) {
        statuses.Add(effect, duration);
    }
}

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

    // abilities
    public delegate void AbilityEffect();
    private AbilityEffect[] abilities;
    private float[] cooldownStats; // amount of time each ability goes on cooldown
    private float[] cooldownTimers; // timer for how much longer the ability is on cooldown
    
    // how is health handled?

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        abilities = new AbilityEffect[3];
        cooldownStats = new float[3];
        cooldownTimers = new float[3];
        ChildStart();
    }
    protected abstract void ChildStart();

    // function for children to add their abilities in their ChildStart() method
    protected void AddAbility(int slot, AbilityEffect effectFunction, float cooldown) {
        abilities[slot] = effectFunction;
        cooldownStats[slot] = cooldown;
    }

    // Update is called once per frame
    void Update()
    {
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

        // using abilities
        int abilitySlot = controller.GetUsedAbility();
        if(abilitySlot >= 0 && abilities[abilitySlot] != null && cooldownTimers[abilitySlot] <= 0) { // if using an ability and that ability is off cooldown
            abilities[abilitySlot]();
            cooldownTimers[abilitySlot] = cooldownStats[abilitySlot];
        }
    }
}

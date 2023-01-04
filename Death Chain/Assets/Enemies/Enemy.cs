using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private int BaseHealth;

    [SerializeField] protected Sprite[] idleSprites;
    [SerializeField] protected Sprite[] walkSprites;
    [SerializeField] protected Sprite[] deathSprites;
    protected Animation currentAnimation;
    protected Animation idleAnimation;
    protected Animation walkAnimation;
    protected Animation deathAnimation;
    private bool UsingAbilityAnimation() { return currentAnimation != null && currentAnimation != idleAnimation && currentAnimation != walkAnimation; }

    private Rigidbody2D body;
    private Statuses statuses = new Statuses(); // conveniently track all status effects
    private float poisonTimer; // tracks when to deal poison damage
    private bool knocked = false; // true means movement is locked as this is being pushed
    private float corpseTimer; // for ai enemies that die
    
    protected int health;
    protected bool isAlly = false; // whether or not this is fighting for the player
    protected bool sturdy = false; // true means this enemy cannot receive knockback
    protected float maxSpeed; // how fast this character can move without factoring in status effects. Can be changed by own abilities
    protected bool invincible; // some abilities need temporary invincibility
    protected Controller controller;

    public int Health { get { return health; } }
    public float DamageMultiplier { get { 
            return 1 + (statuses.HasStatus(Status.Strength) ? 0.5f : 0) - (statuses.HasStatus(Status.Weakness) ? 0.5f : 0); } }
    public bool IsAlly { get { return isAlly; } }
    public bool IsPlayer { get { return controller is PlayerController; } }
    public bool IsCorpse { get { return corpseTimer > 0; } }
    public bool DeleteThis { get; set; } // tells the entity tracker to delete this and remove it from the list

    // Start is called before the first frame update
    void Start()
    {
        health = BaseHealth;
        body = GetComponent<Rigidbody2D>();
        GameObject.Find("EntityTracker").GetComponent<EntityTracker>().AddEnemy(gameObject); // auto add this to the tracker
        currentAnimation = idleAnimation;

        ChildStart();
    }
    protected abstract void ChildStart(); // set up animations here

    // called by an AI controller, allows the enemy script to describe how its AI should work (queue attacks or choose movement modes)
    public virtual void AIUpdate(AIController controller) { }

    // Update is called once per frame
    void Update()
    {
        if(currentAnimation != null) {
            currentAnimation.Update(GetComponent<SpriteRenderer>());
        }

        if(IsCorpse) {
            corpseTimer -= Time.deltaTime;
            if(corpseTimer <= 0) {
                // delete corpse after some time
                DeleteThis = true;
            }
            return;
        }
        else if(corpseTimer < 0) { // if rezzing
            corpseTimer += Time.deltaTime;
            if(corpseTimer > 0) {
                corpseTimer = 0;
                invincible = false;
            }
            return;
        }

        controller.Update();
        statuses.Update();

        // assume idle animation unless mid-ability
        if(idleAnimation != null && !UsingAbilityAnimation()) {
            currentAnimation = idleAnimation;
        }

        // apply friction
        const float FRICTION = 20;
        if(body.velocity != Vector2.zero) {
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

                    // flip sprite to face move direction
                    if(moveDirection.x > 0) {
                        GetComponent<SpriteRenderer>().flipX = false;
                    } 
                    else if(moveDirection.x < 0) {
                        GetComponent<SpriteRenderer>().flipX = true;
                    }

                    // use walk animation unless mid-ability
                    if(walkAnimation != null && !UsingAbilityAnimation()) {
                        currentAnimation = walkAnimation;
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
                TakeDamage(1, true); // damage per tick
            }
        }
    }

    protected abstract void UpdateAbilities();

    public virtual void TakeDamage(int amount, bool ignoreStatus = false) {
        if(invincible) {
            return;
        }

        if(!ignoreStatus) {
            if(statuses.HasStatus(Status.Vulnerability)) {
                amount *= 2;
            }
            if(statuses.HasStatus(Status.Resistance)) {
                amount /= 2;
            }
        }

        health -= amount;
        Debug.Log(health);

        // check for death
        if(health <= 0) {
            body.velocity = Vector2.zero;

            if(!IsPlayer) {
                // become a corpse that can be possessed
                corpseTimer = 5.0f; // corpse duration
                GetComponent<CircleCollider2D>().enabled = false; // disable collider

                // play death animation
                if(deathAnimation != null) {
                    currentAnimation = deathAnimation;
                    currentAnimation.ChangeType(AnimationType.Forward); // make sure it is forward because rezzing plays it backwards
                } else {
                    // temporary
                    GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
            // player death handled by PlayerScript.cs
        }
    }

    public void Push(Vector2 force) {
        if(sturdy || invincible) {
            return;
        }

        knocked = true;
        body.velocity += force;
    }

    // apply a status effect for some time. If no time parameter is given, it is set to an hour to represent infinite duration
    public void ApplyStatus(Status effect, float duration = 60 * 60) {
        statuses.Add(effect, duration);
    }

    public void Possess(PlayerController player) {
        controller = player;
        health = BaseHealth; // reset health

        // become non-corpse
        corpseTimer = -0.5f; // negative indicates resurrection time
        invincible = true; // don't take damage in the middle of ressurrecting 
        if(deathAnimation != null) {
            currentAnimation = deathAnimation;
            currentAnimation.ChangeType(AnimationType.Reverse);
        } else {
            // temporary
            GetComponent<SpriteRenderer>().color = Color.white;
        }

        GetComponent<CircleCollider2D>().enabled = true; // enable collider
        GetComponent<Rigidbody2D>().mass = 0.000001f; // prevent walking through other enemies
    }
}

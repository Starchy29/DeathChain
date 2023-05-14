using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public enum State {
        Normal,
        Corpse,
        Despawing, // play death animation, then despawn
        Resurrect, // animation that plays when first possessing
        Falling, // falling in a pit
    }

    [SerializeField] private GameObject corpseParticle;
    [SerializeField] private GameObject hitParticle;

    [SerializeField] private int BaseHealth;
    [SerializeField] private int BaseSpeed;
    [SerializeField] protected Sprite[] idleSprites;
    [SerializeField] protected Sprite[] walkSprites;
    [SerializeField] protected Sprite[] deathSprites;

    protected Animation currentAnimation;
    protected Animation idleAnimation;
    protected Animation walkAnimation;
    protected Animation deathAnimation;

    private State state = State.Normal;
    private Rigidbody2D body;
    private Statuses statuses; // conveniently track all status effects
    private float poisonTimer; // tracks when to deal poison damage
    private bool knocked = false; // true means movement is locked as this is being pushed
    private float maxSpeed; // how fast this character can move without factoring in status effects. Can be changed by own abilities
    private Timer endlag;
    private float startSize; // assumes width and height are equal
    private Vector3 positionAfterFall; // for fall in pit mechanic

    protected int health;
    protected bool isAlly = false; // whether or not this is fighting for the player
    protected bool sturdy = false; // true means this enemy cannot receive knockback
    protected bool floating = false; // floating enemies can walk over pits
    protected bool invincible; // some abilities need temporary invincibility
    protected Controller controller;
    protected float[] cooldowns = new float[3];

    public int Health { get { return health; } }
    public float WalkSpeed { get { return BaseSpeed; } } // used for AI controller
    public float DamageMultiplier { get { return 1 + (statuses.HasStatus(Status.Strength) ? 0.5f : 0) - (statuses.HasStatus(Status.Weakness) ? 0.5f : 0); } }
    public bool IsAlly { get { return isAlly; } }
    public bool IsPlayer { get { return controller is PlayerController; } }
    public bool Floating { get { return floating; } }
    public State CurrentState { get { return state; } }
    public float CollisionRadius { get { return startSize * GetComponent<CircleCollider2D>().radius; } }
    public bool IsCorpse { get { return state == State.Corpse && (deathAnimation == null || currentAnimation.Done); } }
    public bool DeleteThis { get; set; } // tells the entity tracker to delete this and remove it from the list

    void Start()
    {
        statuses = new Statuses(gameObject);
        health = BaseHealth;
        maxSpeed = BaseSpeed;
        startSize = transform.localScale.x;
        body = GetComponent<Rigidbody2D>();
        EntityTracker.Instance.GetComponent<EntityTracker>().AddEnemy(gameObject); // auto add this to the tracker
        currentAnimation = idleAnimation;

        ChildStart();
    }

    void Update()
    {
        if(currentAnimation != null) {
            currentAnimation.Update(GetComponent<SpriteRenderer>());

            // check for an ability animation finishing
            if(UsingAbilityAnimation() && currentAnimation.Done) {
                currentAnimation = idleAnimation;
            }
        }

        switch(state) {
            case State.Normal:
                controller.Update();
                statuses.Update();

                if(!sturdy) {
                    DoMovement();
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
                } else {
                    poisonTimer = 0;
                }
                
                // decrease cooldowns
                for(int i = 0; i < 3; i++) {
                    if(cooldowns[i] > 0) {
                        cooldowns[i] -= Time.deltaTime;
                        if(cooldowns[i] < 0) {
                            cooldowns[i] = 0;
                        }
                    }
                }
                break;

            case State.Falling:
                const float DURATION_SECONDS = 1.0f;
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                float newAlpha = sprite.color.a - DURATION_SECONDS * Time.deltaTime;
                if(newAlpha <= 0) {
                    state = State.Normal;
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
                    transform.localScale = new Vector3(startSize, startSize, 1);
                    GetComponent<CircleCollider2D>().enabled = true;
                    transform.position = positionAfterFall;
                    TakeDamage(1, true);
                } else {
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
                    float newScale = transform.localScale.x - startSize * (DURATION_SECONDS * 0.7f) * Time.deltaTime;
                    transform.localScale = new Vector3(newScale, newScale, 1);
                }
                break;

            case State.Resurrect:
                if(currentAnimation.Done) {
                    invincible = false;
                    currentAnimation = idleAnimation; // allow normal animations again
                    state = State.Normal;
                }
                break;

            case State.Despawing:
                if(currentAnimation.Done) {
                    DeleteThis = true;
                    OnDeath();

                    GameObject corpse = Instantiate(corpseParticle);
                    corpse.transform.position = transform.position;
                }
                break;
        }
    }

    private void DoMovement() {
        // assume idle animation unless mid-ability
        if(idleAnimation != null && !UsingAbilityAnimation()) {
            currentAnimation = idleAnimation;
        }

        // apply friction
        const float FRICTION = 20;
        if(body.velocity != Vector2.zero) {
            Vector2 friction =  Time.deltaTime * -FRICTION * body.velocity.normalized;
            body.velocity += friction;
            
            // check if friction made this start moving backwards
            if(Vector2.Dot(body.velocity, friction) > 0) {
                body.velocity = Vector2.zero;
            }
        }

        if(knocked) {
            // check for end of knockback
            if(body.velocity.sqrMagnitude <= maxSpeed * maxSpeed) {
                knocked = false;
            }
            return;
        }

        // regular movement
        float currentMaxSpeed = maxSpeed;
        if(statuses.HasStatus(Status.Freeze)) {
            currentMaxSpeed = 0;
        } else {
            currentMaxSpeed *= (statuses.HasStatus(Status.Speed) ? 1.5f : 1) * (statuses.HasStatus(Status.Slow) ? 0.5f : 1);
        }

        Vector2 moveDirection = controller.GetMoveDirection();
        if(maxSpeed <= 0 || moveDirection == Vector2.zero) {
            return;
        }

        const float ACCEL = 80;
        body.velocity +=  Time.deltaTime * ACCEL * moveDirection;
            
        // cap speed
        if(body.velocity.sqrMagnitude > currentMaxSpeed * currentMaxSpeed) {
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
        
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-transform.position.y * 10); // draw lower characters in front
    }

    public virtual void TakeDamage(int amount, bool ignoreStatus = false) {
        if(invincible || amount <= 0) {
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
        if(!ignoreStatus) {
            GameObject hitEffect = Instantiate(hitParticle, transform);
            hitEffect.transform.localScale = new Vector3(1.25f / transform.localScale.x, 1.25f / transform.localScale.y, 1);
            hitEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        // check for death
        if(health <= 0) {
            body.velocity = Vector2.zero;
            statuses.ClearPoison();
            for(int i = 0; i < 3; i++) {
                cooldowns[i] = 0;
            }

            if(!IsPlayer) {
                // become a corpse that can be possessed
                state = State.Corpse;
                //Timer.CreateTimer(5.0f, false, () => { // despawn corpse after some time
                //    if(state == State.Corpse) { // don't delete if resurrected
                //        DeleteThis = true;
                //        GameObject corpse = Instantiate(corpseParticle);
                //        corpse.transform.position = transform.position;
                //    }
                //});
                Timer.CreateTimer(0.6f, false, OnDeath); // use optional death effect after 0.6 seconds of dying
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
            // else: player death handled by PlayerScript.cs
        }
    }

    public void Push(Vector2 force) {
        if(sturdy || invincible) {
            return;
        }

        knocked = true;
        body.velocity = force;
    }

    // apply a status effect for some time. If no time parameter is given, it is set to an hour to represent infinite duration
    public void ApplyStatus(Status effect, float duration = 60 * 60) {
        if(invincible && effect == Status.Poison) {
            return;
        }

        statuses.Add(effect, duration);
    }

    public void Possess(PlayerController player) {
        controller = player;
        health = BaseHealth; // reset health
        isAlly = true;

        // become non-corpse
        state = State.Resurrect;
        invincible = true; // don't take damage in the middle of ressurrecting 
        if(deathAnimation != null) {
            currentAnimation = deathAnimation;
            currentAnimation.ChangeType(AnimationType.Reverse);
            currentAnimation.AddPause(0.4f);
        } else {
            // temporary
            GetComponent<SpriteRenderer>().color = Color.white;
            invincible = false;
            state = State.Normal;
        }

        GetComponent<CircleCollider2D>().enabled = true; // enable collider
        GetComponent<Rigidbody2D>().mass = 0.000001f; // prevent walking through other enemies
    }

    public void Unpossess() {
        // temporary
        if(deathAnimation == null) {
            DeleteThis = true;
            return;
        }

        state = State.Despawing;
        controller = null; // just to be safe
        GetComponent<CircleCollider2D>().enabled = false;
        currentAnimation = deathAnimation;
        currentAnimation.ChangeType(AnimationType.Forward);
        body.velocity = Vector2.zero;
    }

    public void FallInPit(Vector3 positionAfterFall) {
        state = State.Falling;
        GetComponent<CircleCollider2D>().enabled = false;
        body.velocity = Vector3.zero;
        this.positionAfterFall = positionAfterFall;
    }

// Functions for sub classes
    protected abstract void ChildStart();
    protected abstract void UpdateAbilities();

    // called by an AI controller, allows the enemy script to describe how its AI should work (queue attacks or choose movement modes)
    public virtual void AIUpdate(AIController controller) { }
    protected virtual void OnDeath() { }

    protected bool UseAbility(int ability) {
        return (endlag == null || !endlag.Active) && cooldowns[ability] <= 0 && controller.AbilityUsed(ability);
    }

    protected GameObject CreateAttack(GameObject prefab) {
        GameObject attack = Instantiate(prefab);
        attack.transform.position = transform.position; // defualt placement is directly on top
        Attack script = attack.GetComponent<Attack>();
        script.User = gameObject;

        if(script is Projectile projectileScript) {
            // for projectiles, default aim to the controller's aim
            projectileScript.SetDirection(controller.GetAimDirection());
        }

        return attack;
    }

    // create a time period after using an attack where the character moves slower
    protected void ApplyEndlag(float duration, float tempSpeed) {
        if(duration < 0 || tempSpeed < 0) {
            return;
        }
        endlag = Timer.CreateTimer(duration, false, () => { maxSpeed = BaseSpeed; });
        maxSpeed = tempSpeed;
    }

    protected void Dash(Vector2 velocity, float duration) {
        if(duration <= 0) {
            return;
        }

        body.velocity = velocity;
        sturdy = true;
        Timer.CreateTimer(duration, false, () => {
            sturdy = false;
            body.velocity = Vector2.zero;
        });
    }

    private bool UsingAbilityAnimation() { 
        return currentAnimation != null && currentAnimation != idleAnimation && currentAnimation != walkAnimation && currentAnimation != deathAnimation;
    }
}

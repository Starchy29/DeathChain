using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{

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
    private bool UsingAbilityAnimation() { return currentAnimation != null && currentAnimation != idleAnimation && currentAnimation != walkAnimation && currentAnimation != deathAnimation; }

    private Rigidbody2D body;
    private Statuses statuses = new Statuses(); // conveniently track all status effects
    private float poisonTimer; // tracks when to deal poison damage
    private bool knocked = false; // true means movement is locked as this is being pushed
    private float corpseTimer; // for ai enemies that die
    private float maxSpeed; // how fast this character can move without factoring in status effects. Can be changed by own abilities
    private float endlag;

    protected int health;
    protected bool isAlly = false; // whether or not this is fighting for the player
    protected bool sturdy = false; // true means this enemy cannot receive knockback
    protected bool invincible; // some abilities need temporary invincibility
    protected Controller controller;
    protected float[] cooldowns = new float[3];

    public int Health { get { return health; } }
    public float WalkSpeed { get { return BaseSpeed; } } // used for AI controller
    public float DamageMultiplier { get { return 1 + (statuses.HasStatus(Status.Strength) ? 0.5f : 0) - (statuses.HasStatus(Status.Weakness) ? 0.5f : 0); } }
    public bool IsAlly { get { return isAlly; } }
    public bool IsPlayer { get { return controller is PlayerController; } }
    public bool IsCorpse { get { return corpseTimer > 0; } }
    public bool Possessable { get { return IsCorpse && (deathAnimation == null || currentAnimation.Done); } } // prevent possessing during death animation
    public bool DeleteThis { get; set; } // tells the entity tracker to delete this and remove it from the list
    public bool Invincible { get { return invincible; } }

    // Start is called before the first frame update
    void Start()
    {
        health = BaseHealth;
        maxSpeed = BaseSpeed;
        body = GetComponent<Rigidbody2D>();
        EntityTracker.Instance.GetComponent<EntityTracker>().AddEnemy(gameObject); // auto add this to the tracker
        currentAnimation = idleAnimation;

        ChildStart();
    }
    protected abstract void ChildStart();

    // called by an AI controller, allows the enemy script to describe how its AI should work (queue attacks or choose movement modes)
    public virtual void AIUpdate(AIController controller) { }

    // Update is called once per frame
    void Update()
    {
        if(currentAnimation != null) {
            currentAnimation.Update(GetComponent<SpriteRenderer>());

            // check for an ability animation finishing
            if(UsingAbilityAnimation() && currentAnimation.Done) {
                currentAnimation = idleAnimation;
            }
        }

        if(IsCorpse) { // dead body that the player can possess
            float previousTime = corpseTimer;
            corpseTimer -= Time.deltaTime;

            if(previousTime > 4.4 && corpseTimer <= 4.4) {
                // use optional death effect after 0.6 seconds of dying
                OnDeath();
            }
            else if(corpseTimer <= 0) {
                // delete corpse after some time
                DeleteThis = true;
                GameObject corpse = Instantiate(corpseParticle);
                corpse.transform.position = transform.position;
            }
            return;
        }
        else if(controller == null) { // this is doing its death animation, then despawning
            if(currentAnimation.Done) {
                DeleteThis = true;
                OnDeath();

                GameObject corpse = Instantiate(corpseParticle);
                corpse.transform.position = transform.position;
            }
            return;
        }
        else if(deathAnimation != null && currentAnimation == deathAnimation) { // playing resurrection animation when first possessed
            if(currentAnimation.Done) {
                invincible = false;
                currentAnimation = idleAnimation; // allow normal animations again
            }
            return;
        }

        controller.Update();
        statuses.Update();

        // assume idle animation unless mid-ability
        if(idleAnimation != null && !UsingAbilityAnimation()) {
            currentAnimation = idleAnimation;
        }

        if(!sturdy) {
            DoMovement();
        }

        // abilities handled in each sub class
        UpdateAbilities();

        if(endlag > 0) {
            endlag -= Time.deltaTime;
            if(endlag <= 0) {
                maxSpeed = BaseSpeed;
            }
        }

        // manage poison damage
        if(statuses.HasStatus(Status.Poison)) {
            poisonTimer -= Time.deltaTime;
            if(poisonTimer <= 0) {
                poisonTimer += 0.5f; // poison tick rate
                TakeDamage(1, true); // damage per tick
            }
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
    }

    private void DoMovement() {
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
        
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-transform.position.y * 10); // draw lower characters in front
    }

    protected abstract void UpdateAbilities();
    protected virtual void OnDeath() { }

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
        Debug.Log(health);
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
        corpseTimer = 0;
        invincible = true; // don't take damage in the middle of ressurrecting 
        if(deathAnimation != null) {
            currentAnimation = deathAnimation;
            currentAnimation.ChangeType(AnimationType.Reverse);
            currentAnimation.AddPause(0.4f);
        } else {
            // temporary
            GetComponent<SpriteRenderer>().color = Color.white;
            invincible = false;
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

        controller = null;
        GetComponent<CircleCollider2D>().enabled = false;
        currentAnimation = deathAnimation;
        currentAnimation.ChangeType(AnimationType.Forward);
        body.velocity = Vector2.zero;
    }

    // helper functions for sub classes
    protected bool UseAbility(int ability) {
        return endlag <= 0 && cooldowns[ability] <= 0 && controller.AbilityUsed(ability);
    }

    protected GameObject CreateAttack(GameObject prefab) {
        GameObject attack = Instantiate(prefab);
        attack.transform.position = transform.position; // defualt placement is directly on top
        Attack script = attack.GetComponent<Attack>();
        script.User = gameObject;

        if(script is Projectile) {
            // for projectiles, default aim to the controller's aim
            ((Projectile)script).SetDirection(controller.GetAimDirection());
        }

        return attack;
    }

    // create a time period after using an attack where the character moves slower
    protected void ApplyEndlag(float duration, float tempSpeed) {
        if(duration < 0 || tempSpeed < 0) {
            return;
        }
        endlag = duration;
        maxSpeed = tempSpeed;
    }

    protected void Dash(Vector2 velocity) {
        body.velocity = velocity;
        sturdy = true;
    }

    protected void EndDash() {
        sturdy = false;
        body.velocity = Vector2.zero;
    }
}

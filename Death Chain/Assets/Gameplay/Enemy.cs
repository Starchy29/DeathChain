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

    [SerializeField] private int BaseHealth;
    [SerializeField] private float BaseSpeed;
    [SerializeField] private int difficulty; // 1-3
    [SerializeField] protected Sprite[] idleSprites;
    [SerializeField] protected Sprite[] walkSprites;
    [SerializeField] protected Sprite[] deathSprites;

    protected Animation currentAnimation;
    protected Animation idleAnimation;
    protected Animation walkAnimation;
    protected Animation deathAnimation;
    protected const float DEATH_ANIM_DURATION = 0.6f;

    private State state = State.Normal;
    private Rigidbody2D body;
    private float poisonTimer; // tracks when to deal poison damage
    private bool knocked = false; // true means movement is locked as this is being pushed
    private float maxSpeed; // how fast this character can move without factoring in status effects. Can be changed by own abilities
    private Timer endlag;
    private float startSize; // assumes width and height are equal
    private bool faceLocked; // prevents changing the face direction from moving
    private Vector3 positionAfterFall; // for fall in pit mechanic
    private Timer dashTimer; // if non-null, this is currently dashing

    protected int health;
    protected Statuses statuses; // conveniently track all status effects
    protected bool showAimer = false;
    protected bool isAlly = false; // whether or not this is fighting for the player, only change with IsAlly property
    protected bool sturdy = false; // true means this enemy cannot receive knockback
    protected bool floating = false; // floating enemies can walk over pits
    protected bool invincible; // some abilities need temporary invincibility
    protected Controller controller;
    protected float[] cooldowns = new float[3];
    protected bool Dashing { get { return dashTimer != null; } }

    public float WalkSpeed { get { 
        return maxSpeed
            * (statuses.HasStatus(Status.Speed) ? 1.5f : 1)
            * (statuses.HasStatus(Status.Slow) ? 0.5f : 1)
            * (statuses.HasStatus(Status.Freeze) ? 0 : 1);
    }}
    public int Health { get { return health; } }
    public int Difficulty { get { return difficulty; } }
    public float DamageMultiplier { get { return 1 + (statuses.HasStatus(Status.Strength) ? 0.5f : 0) - (statuses.HasStatus(Status.Weakness) ? 0.5f : 0); } }
    public float[] Cooldowns { get { return cooldowns; } }
    public bool IsAlly {
        get { return isAlly; } 
        private set { 
            isAlly = value; 
            gameObject.layer = LayerMask.NameToLayer((isAlly ? "PlayerAlly" : "Enemy")); 
        } 
    }
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
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-transform.position.y * 10); // repeated whenever the character changes position
        
        ChildStart();
        if(currentAnimation == null) { // allows children to choose a different start animation
            currentAnimation = idleAnimation; // animations are created by child classes
        }

        gameObject.layer = LayerMask.NameToLayer((controller is PlayerController ? "PlayerAlly" : "Enemy"));
    }

    void Update()
    {
        if(PauseMenuScript.Instance.Paused) {
            return;
        }

        if(currentAnimation != null) {
            currentAnimation.Update(GetComponent<SpriteRenderer>());

            // check for an ability animation finishing
            if(UsingAbilityAnimation() && currentAnimation.Done) {
                StartAnimation(idleAnimation);
                faceLocked = false;
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
                const float POISON_RATE = 1.0f;
                if(statuses.HasStatus(Status.Poison)) {
                    poisonTimer -= Time.deltaTime;
                    if(poisonTimer <= 0) {
                        poisonTimer += POISON_RATE;
                        TakeDamage(1, true); // damage per tick
                    }
                } else {
                    poisonTimer = POISON_RATE / 3; // the first poison damage happens sooner than normal
                }
                
                // decrease cooldowns
                for(int i = 0; i < 3; i++) {
                    if(cooldowns[i] > 0) {
                        cooldowns[i] -= Time.deltaTime * (statuses.HasStatus(Status.Energy) ? 1.5f : 1);
                        if(cooldowns[i] < 0) {
                            cooldowns[i] = 0;
                        }
                    }
                }

                // place aimer if this is a player
                if(IsPlayer) {
                    if(showAimer) {
                        PlayerScript.Instance.Aimer.SetActive(true);
                        PlayerScript.Instance.Aimer.transform.position = transform.position + 1.5f * (Vector3)controller.GetAimDirection();
                    } else {
                        PlayerScript.Instance.Aimer.SetActive(false);
                    }
                }
                break;

            case State.Falling:
                const float DURATION_SECONDS = 1.0f;
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                float newAlpha = sprite.color.a - DURATION_SECONDS * Time.deltaTime;
                if(newAlpha <= 0) {
                    // end fall
                    if(currentAnimation != null && currentAnimation == deathAnimation) {
                        // if a floating enemy dies over a pit, just despawn it
                        DeleteThis = true;
                    } else {
                        state = State.Normal;
                        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
                        transform.localScale = new Vector3(startSize, startSize, 1);
                        GetComponent<CircleCollider2D>().enabled = true;
                        transform.position = positionAfterFall;
                        TakeDamage(2, true);
                    }
                } else {
                    // shrink and fade out
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

                    GameObject corpse = Instantiate(IconsAndEffects.Instance.CorpseParticle);
                    corpse.transform.position = transform.position;
                }
                break;
        }
    }

    private void DoMovement() {
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
        
        float currentMaxSpeed = WalkSpeed;

        if(knocked) {
            // check for end of knockback
            if(body.velocity.sqrMagnitude <= currentMaxSpeed * currentMaxSpeed) {
                knocked = false;
            }
            return;
        }

        // regular movement
        Vector2 moveDirection = ModifyDirection(controller.GetMoveDirection());
        if(maxSpeed <= 0 || moveDirection == Vector2.zero) {
            if(currentAnimation != idleAnimation && idleAnimation != null && !UsingAbilityAnimation()) {
                StartAnimation(idleAnimation);
            }
            return;
        }

        const float ACCEL = 80;
        body.velocity +=  Time.deltaTime * ACCEL * moveDirection;
            
        // cap speed
        if(body.velocity.sqrMagnitude > currentMaxSpeed * currentMaxSpeed) {
            body.velocity = body.velocity.normalized;
            body.velocity *= currentMaxSpeed;
        }

        // flip sprite to face move direction
        if(!faceLocked) {
            if(moveDirection.x > 0) {
                GetComponent<SpriteRenderer>().flipX = false;
            } 
            else if(moveDirection.x < 0) {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        // use walk animation unless mid-ability
        if(currentAnimation != walkAnimation && walkAnimation != null && !UsingAbilityAnimation()) {
            StartAnimation(walkAnimation);
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
            GameObject hitEffect = Instantiate(IconsAndEffects.Instance.HitParticle, transform);
            hitEffect.transform.localScale = new Vector3(1.25f / transform.localScale.x, 1.25f / transform.localScale.y, 1);
            hitEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        if(health > 0) {
            return;
        }

        // die
        body.velocity = Vector2.zero;
        statuses.ClearPoison();
        ResetAndClear();
        for (int i = 0; i < 3; i++) {
            cooldowns[i] = 0;
        }

        if(IsPlayer) {
            // player death handled by PlayerScript.cs
            return;
        }

        PlayerScript.Instance.AddResources(difficulty);

        // become a corpse that can be possessed
        state = State.Corpse;
        Timer.CreateTimer(gameObject, 10.0f, false, () => { // despawn corpse after some time
            if(state == State.Corpse) { // don't delete if resurrected
                DeleteThis = true;
                GameObject corpse = Instantiate(IconsAndEffects.Instance.CorpseParticle);
                corpse.transform.position = transform.position;
            }
        });
        Timer.CreateTimer(gameObject, 0.6f, false, OnDeath); // use optional death effect after 0.6 seconds of dying
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
        ResetWalkSpeed(); // in case the enemy changed its own speed
        IsAlly = true;

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

        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().mass = 0.000001f; // prevent walking through other enemies
        if(sturdy) {
            GetComponent<Rigidbody2D>().mass = 100f; // enemies that cannot move should instead have a big mass so enemies cannot move through the player
        }
    }

    public void Unpossess() {
        if(deathAnimation == null) {
            // temporary
            DeleteThis = true;
            return;
        }

        state = State.Despawing;
        controller = null;
        GetComponent<CircleCollider2D>().enabled = false;
        currentAnimation = deathAnimation;
        currentAnimation.ChangeType(AnimationType.Forward);
        body.velocity = Vector2.zero;
        ResetAndClear();
    }

    public void FallInPit(Vector3 positionAfterFall) {
        state = State.Falling;
        GetComponent<CircleCollider2D>().enabled = false;
        body.velocity = Vector3.zero;
        this.positionAfterFall = positionAfterFall;
        EndDash();
    }

    #region Functions for sub-classes
    protected abstract void ChildStart();
    protected abstract void UpdateAbilities();
    protected virtual void OnDeath() { } // for special abilities when dying
    protected virtual void ResetAndClear() { } // Remove any game objects that this enemy tracks and reset from special states

    // called by an AI controller, allows the enemy script to describe how its AI should work (queue attacks or choose movement modes)
    public virtual void AIUpdate(AIController controller) { }

    protected void StartAnimation(Animation newAnimation) {
        if(newAnimation == null) {
            return;
        }

        currentAnimation = newAnimation;
        currentAnimation.Reset();
    }

    protected bool UseAbility(int ability) {
        return (endlag == null || !endlag.Active) && cooldowns[ability] <= 0 && controller.AbilityUsed(ability);
    }

    protected GameObject CreateAbility(GameObject prefab, bool faceAttack = false) {
        GameObject ability = Instantiate(prefab);
        ability.transform.position = transform.position; // default placement is directly on top

        Vector2 aim = Vector2.zero;
        if(controller != null) {
            aim = controller.GetAimDirection();
        }

        Ability abilityScript = ability.GetComponent<Ability>();
        abilityScript.User = this;
        abilityScript.SetDirection(aim); // default aim to the character's current aim

        if (faceAttack) {
            faceLocked = true;
            if(aim.x > 0) {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if(aim.x < 0) {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        return ability;
    }

    // create a time period after using an attack where the character moves slower
    protected void ApplyEndlag(float duration, float tempSpeed) {
        if(duration < 0 || tempSpeed < 0) {
            return;
        }
        endlag = Timer.CreateTimer(gameObject, duration, false, () => { maxSpeed = BaseSpeed; });
        maxSpeed = tempSpeed;
    }

    // functions for altering the character's walk speed
    protected void SetWalkSpeed(float speed) {
        maxSpeed = speed;
    }
    protected void ResetWalkSpeed() {
        maxSpeed = BaseSpeed;
    }

    protected void Dash(Vector2 velocity, float duration, float endlag = 0) {
        if(duration <= 0) {
            return;
        }

        body.velocity = velocity;
        GetComponent<SpriteRenderer>().flipX = velocity.x < 0;

        sturdy = true;
        dashTimer = Timer.CreateTimer(gameObject, duration, false, () => {
            body.velocity = Vector2.zero;

            if(endlag > 0) {
                dashTimer = Timer.CreateTimer(gameObject, endlag, false, EndDash);
            } else {
                EndDash();
            }
        });
    }

    // ends the current dash. Does nothing if the enemy is not dashing
    protected void EndDash() {
        if(!Dashing) {
            return;
        }

        sturdy = false;
        dashTimer.End();
        dashTimer = null;
        StartAnimation(idleAnimation);
    }

    // allows enemies to have special movement patterns. Recieves the controller's direction and returns a new direction to move in
    protected virtual Vector2 ModifyDirection(Vector2 direction) {
        return direction;
    }
    #endregion

    public void BecomeMiniboss() {
        // this function is usually called before Start()
        Timer.CreateTimer(gameObject, 0.1f, false, () => {
            statuses = new Statuses(gameObject); 
            statuses.Add(Status.Energy, 3600, false);
            statuses.Add(Status.Strength, 3600, false);
            statuses.Add(Status.Speed, 3600, false);
            statuses.Add(Status.Resistance, 3600, false);
            startSize *= 1.5f;;
            transform.localScale = new Vector3(startSize, startSize, 1);
        });
    }

    private bool UsingAbilityAnimation() { 
        return currentAnimation != null && currentAnimation != idleAnimation && currentAnimation != walkAnimation && currentAnimation != deathAnimation;
    }
}

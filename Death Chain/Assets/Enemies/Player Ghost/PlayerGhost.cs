using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    private const float BASE_WALK_SPEED = 5.0f;
    private const float ATTACK_WALK_SPEED = 2.0f;
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] slashSprites;
    [SerializeField] private Sprite[] unpossessSprites;
    [SerializeField] private float slashCooldown;
    [SerializeField] private float shootCooldown;
    private Animation slashAnimation;
    private Animation shootAnimation;
    private float invulnTimer;
    private bool useUnpossessAnim; // marks when this should use the special animation

    public GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing
    private bool clockwise; // which way the slash should go, alternates every time

    public GameObject ShotPrefab;

    protected override void ChildStart()
    {
        controller = new PlayerController(gameObject);
        isAlly = true;
        maxSpeed = BASE_WALK_SPEED;

        idleAnimation = new Animation(idleSprites, AnimationType.Loop, 0.4f);
        walkAnimation = idleAnimation;
        shootAnimation = new Animation(shootSprites, AnimationType.Rebound, 0.15f);

        if(useUnpossessAnim) {
            GetComponent<SpriteRenderer>().sprite = unpossessSprites[0];
            currentAnimation = new Animation(unpossessSprites, AnimationType.Forward, 0.4f);
        }
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null) {
            // slash updates on its own

            if(currentSlash.GetComponent<MeleeSwipe>().Finished) {
                Destroy(currentSlash);
                currentSlash = null;
                maxSpeed = BASE_WALK_SPEED;
            }
        } else {
                if(UseAbility(0)) { // slash
                    cooldowns[0] = slashCooldown;
                    clockwise = !clockwise;
                    clockwise = true;
                    maxSpeed = ATTACK_WALK_SPEED; // slow while slashing

                    currentSlash = Instantiate(SlashPrefab);
                    MeleeSwipe slashScript = currentSlash.GetComponent<MeleeSwipe>();
                    slashScript.User = this.gameObject;
                    slashScript.SetAim(controller.GetAimDirection(), clockwise);
                }
                else if(UseAbility(1)) { // shoot
                    cooldowns[1] = shootCooldown;
                    GameObject shot = Instantiate(ShotPrefab);
                    shot.transform.position = transform.position;
                    Projectile script = shot.GetComponent<Projectile>();
                    script.User = this.gameObject;
                    Vector2 aimDirection = controller.GetAimDirection();
                    script.SetDirection(aimDirection);

                    maxSpeed = ATTACK_WALK_SPEED;

                    currentAnimation = shootAnimation;
                    currentAnimation.Reset();
                    GetComponent<SpriteRenderer>().flipX = aimDirection.x < 0; // face shoot direction (when unmoving)
                }
        }

        if(maxSpeed == ATTACK_WALK_SPEED && cooldowns[1] <= shootCooldown - 0.2f) {
            maxSpeed = BASE_WALK_SPEED;
        }
        if(invulnTimer > 0) {
            invulnTimer -= Time.deltaTime;
            if(invulnTimer <= 0) {
                invincible = false;
            }
        }
    }

    // when unpossessing, allow the player info tracker to pass the right values for the player
    public void Setup(int health) {
        this.health = health;
        invincible = true;
        invulnTimer = 1.0f;
        useUnpossessAnim = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] slashSprites;
    [SerializeField] private Sprite[] unpossessSprites;
    [SerializeField] private float slashCooldown;
    [SerializeField] private float shootCooldown;
    private Animation slashAnimation;
    private Animation shootAnimation;
    private bool useUnpossessAnim; // marks when this should use the special animation

    public GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing
    private bool clockwise; // which way the slash should go, alternates every time
    private int trueHealth;

    public GameObject ShotPrefab;

    protected override void ChildStart()
    {
        if(trueHealth > 0)
            health = trueHealth; // allow spawning with less health
        controller = new PlayerController(gameObject);
        isAlly = true;

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
            }
            return;
        } 

        if(UseAbility(0)) { // slash
            cooldowns[0] = slashCooldown;
            ApplyEndlag(0.3f, 2.0f);
            clockwise = !clockwise;
            clockwise = true;

            currentSlash = CreateAttack(SlashPrefab);
            currentSlash.GetComponent<MeleeSwipe>().SetAim(controller.GetAimDirection(), clockwise);
        }
        else if(UseAbility(1)) { // shoot
            cooldowns[1] = shootCooldown;
            ApplyEndlag(0.3f, 2.0f);
            CreateAttack(ShotPrefab);

            currentAnimation = shootAnimation;
            currentAnimation.Reset();
            GetComponent<SpriteRenderer>().flipX = controller.GetAimDirection().x < 0; // face shoot direction (when unmoving)
        }
    }

    // when unpossessing, allow the player info tracker to pass the right values for the player
    public void Setup(int health) {
        trueHealth = health; // used to override when health is set in Enemy.Start()
        invincible = true;
        new Timer(1.0f, false, () => { invincible = false; });
        useUnpossessAnim = true;
    }
}

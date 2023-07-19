using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base form of the player
public class PlayerGhost : Enemy
{
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] slashSprites;
    [SerializeField] private Sprite[] unpossessSprites;
    
    private Animation slashAnimation;
    private Animation shootAnimation;

    private const float SLASH_CD = 0.6f;
    private const float SHOOT_CD = 1.0f;

    public GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing
    private int trueHealth;

    public GameObject ShotPrefab;

    protected override void ChildStart()
    {
        if(trueHealth > 0) {
            health = trueHealth; // allow spawning with less health
            // use unpossess animation
            GetComponent<SpriteRenderer>().sprite = unpossessSprites[0];
            currentAnimation = new Animation(unpossessSprites, AnimationType.Forward, 0.4f);
        }
        controller = new PlayerController(gameObject);
        isAlly = true;

        idleAnimation = new Animation(idleSprites, AnimationType.Loop, 0.4f);
        walkAnimation = idleAnimation;
        shootAnimation = new Animation(shootSprites, AnimationType.Rebound, 0.15f);
        slashAnimation = new Animation(slashSprites, AnimationType.Forward, 0.2f);
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null) {
            // slash updates on its own
            return;
        }

        if(UseAbility(0)) { // slash
            cooldowns[0] = SLASH_CD;
            currentSlash = CreateAttack(SlashPrefab, true);
            ApplyEndlag(0.3f, 2.0f);
            currentAnimation = slashAnimation;
            slashAnimation.Reset();
        }
        else if(UseAbility(1)) { // shoot
            cooldowns[1] = SHOOT_CD;
            ApplyEndlag(0.3f, 2.0f);
            CreateAttack(ShotPrefab, true).transform.position += new Vector3(0, 0.3f, 0);

            currentAnimation = shootAnimation;
            currentAnimation.Reset();
        }
    }

    protected override void ResetAndClear() {
        if(currentSlash != null) {
            Destroy(currentSlash);
        }
    }

    // when unpossessing, allow the player info tracker to pass the right values for the player
    public void Setup(int health) {
        trueHealth = health; // used to override when health is set in Enemy.Start()
        invincible = true;
        Timer.CreateTimer(1.0f, false, () => { invincible = false; });
    }
}

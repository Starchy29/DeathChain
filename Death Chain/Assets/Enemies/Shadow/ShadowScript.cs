using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for the enemy type called shadow
public class ShadowScript : Enemy
{
    [SerializeField] private GameObject SlashPrefab;
    [SerializeField] private Sprite[] dashSprites;
    [SerializeField] private Sprite[] slash1Sprites;
    [SerializeField] private Sprite[] slash2Sprites;

    private Animation dashAnimation;
    private Animation slash1Animation;
    private Animation slash2Animation;

    private GameObject currentSlash; // null means not currently slashing
    private const float DASH_CD = 1.0f;
    private bool firstSlash; // false: second slash

    protected override void ChildStart()
    {
        controller = new AIController(gameObject, AIMode.Chase, AIMode.Wander, 5.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Loop, 0.8f);
        walkAnimation = new Animation(walkSprites, AnimationType.Loop, 0.8f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        dashAnimation = new Animation(dashSprites, AnimationType.Loop, 0.3f);
        slash1Animation = new Animation(slash1Sprites, AnimationType.Forward, 0.25f);
        slash2Animation = new Animation(slash2Sprites, AnimationType.Forward, 0.25f);

        firstSlash = true;
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null || Dashing) {
            return;
        }

        if(UseAbility(0)) { // slash
            currentSlash = CreateAttack(SlashPrefab);

            if(firstSlash) {
                currentAnimation = slash1Animation;
                currentAnimation.Reset();
                cooldowns[0] = 0.3f;
                ApplyEndlag(0.3f, 1.0f);
                firstSlash = false;
                Timer.CreateTimer(0.6f, false, () => { firstSlash = true; });
            } else {
                currentAnimation = slash2Animation;
                currentAnimation.Reset();
                cooldowns[0] = 1.0f;
                firstSlash = true;
                currentSlash.GetComponent<Melee>().ReverseDirection();
            }

        }
        else if(UseAbility(1)) { // dash
            cooldowns[1] = DASH_CD;
            Vector2 direction = controller.GetMoveDirection();
            if(direction == Vector2.zero) {
                direction = controller.GetAimDirection();
            }
            Dash(20.0f * direction, 0.14f, 0.08f);
            currentAnimation = dashAnimation;
            currentAnimation.Reset();
        }
    }

    protected override void ResetAndClear() {
        if(currentSlash != null) {
            Destroy(currentSlash);
        }
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target == null || controller.IsTargetBlocked(true)) {
            return;
        }

        if(cooldowns[0] <= 0 && (controller.GetTargetDistance() <= 2.0f || !firstSlash)) {
            float startup = 0.4f;
            if(!firstSlash) {
                startup = 0.0f;
            }
            controller.QueueAbility(0, startup, 0.0f);
        }
        else if(cooldowns[1] <= 0 && controller.GetTargetDistance() >= 4.0f) {
            controller.QueueAbility(1, 0.2f);
        }
    }
}

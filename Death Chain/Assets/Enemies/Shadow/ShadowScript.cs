using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for the enemy type called shadow
public class ShadowScript : Enemy
{
    [SerializeField] private GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing

    private const float DASH_CD = 1.0f;
    private bool firstSlash; // false: second slash

    protected override void ChildStart()
    {
        controller = new AIController(gameObject, AIMode.Chase, AIMode.Wander, 5.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Loop, 0.8f);
        walkAnimation = new Animation(walkSprites, AnimationType.Loop, 0.8f);

        firstSlash = true;
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null || dashing) {
            return;
        }

        if(UseAbility(0)) { // slash
            currentSlash = CreateAttack(SlashPrefab);

            if(firstSlash) {
                cooldowns[0] = 0.3f;
                ApplyEndlag(0.3f, 1.0f);
                firstSlash = false;
                Timer.CreateTimer(0.6f, false, () => { firstSlash = true; });
            } else {
                cooldowns[0] = 1.0f;
                firstSlash = true;
                currentSlash.GetComponent<Melee>().ReverseDirection();
            }

        }
        else if(UseAbility(1)) { // dash
            cooldowns[1] = DASH_CD;
            Dash(20.0f * controller.GetMoveDirection(), 0.12f, 0.1f);
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

        if(cooldowns[0] <= 0 && controller.GetTargetDistance() <= 2.0f) {
            float startup = 0.4f;
            if(!firstSlash) {
                startup = 0.0f;
            }
            controller.QueueAbility(0, startup, 0.0f);
        }
        else if(cooldowns[1] <= 0 && controller.GetTargetDistance() >= 4.0f) {
            controller.QueueAbility(1);
        }
    }
}

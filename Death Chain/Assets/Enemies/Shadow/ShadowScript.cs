using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for the enemy type called shadow
public class ShadowScript : Enemy
{
    [SerializeField] private GameObject SlashPrefab;
    private GameObject currentSlash; // null means not currently slashing

    protected override void ChildStart()
    {
        controller = new AIController(gameObject, AIMode.Wander, 7.0f);
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null) {
            if(currentSlash.GetComponent<MeleeSwipe>().Finished) {
                Destroy(currentSlash);
                currentSlash = null;
                //EndDash();
            }

            return;
        }

        if(UseAbility(0)) { // slash dash
            cooldowns[0] = 2.0f;

            currentSlash = CreateAttack(SlashPrefab);
            Vector2 aim = controller.GetAimDirection();
            currentSlash.GetComponent<MeleeSwipe>().SetAim(aim, true);
            //Dash(aim * 12);
        }
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target == null) {
            controller.MoveMode = AIMode.Wander;
        } else {
            controller.MoveMode = AIMode.Chase;
        }

        if(cooldowns[0] <= 0 && controller.Target != null && controller.GetTargetDistance() <= 4.0f) {
            controller.QueueAbility(0, 0.3f);
        }
    }
}

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
        controller = new AIController(gameObject, AIMode.Wander, 5.0f);
    }

    protected override void UpdateAbilities() {
        if(currentSlash != null) {
            // slash updates automatically
            return;
        }

        if(UseAbility(0)) { // slash dash
            cooldowns[0] = 1.3f;
            currentSlash = CreateAttack(SlashPrefab);
            Dash(12.0f * controller.GetAimDirection(), 0.2f);
        }
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target == null) {
            controller.MoveMode = AIMode.Wander;
        } else {
            controller.MoveMode = AIMode.Chase;
        }

        if(cooldowns[0] <= 0 && controller.Target != null && controller.GetTargetDistance() <= 3.0f) {
            controller.QueueAbility(0, 0.5f, 1.0f);
        }
    }
}

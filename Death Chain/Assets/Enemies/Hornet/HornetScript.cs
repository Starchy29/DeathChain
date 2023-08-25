using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornetScript : Enemy
{
    [SerializeField] private GameObject AttackPrefab;

    private const float ATTACK_CD = 0.4f;
    private const float SPEED_CD = 8.0f;

    private Vector2 lastMoveDirection;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Patrol, AIMode.Patrol, 3.0f);

        floating = true;
        lastMoveDirection = new Vector2(0, -1);
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // contact damage attack
            cooldowns[0] = ATTACK_CD;
            CreateAttack(AttackPrefab).transform.parent = transform;
        }
        else if(UseAbility(1)) {
            // speed boost
            cooldowns[1] = SPEED_CD;
            ApplyStatus(Status.Speed, 3.0f);
        }
    }

    // make the character always move forward
    protected override Vector2 ModifyDirection(Vector2 direction) {
        if(direction != Vector2.zero) {
            lastMoveDirection = direction;
            return direction;
        }

        return lastMoveDirection;
    }

    public override void AIUpdate(AIController controller) {
        if(health <= 3 && cooldowns[1] <= 0) {
            controller.QueueAbility(1);
        }
        else if(controller.Target != null && cooldowns[0] <= 0) {
            controller.QueueAbility(0);
        }
    }
}

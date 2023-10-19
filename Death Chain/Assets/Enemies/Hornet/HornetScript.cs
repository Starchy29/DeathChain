using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornetScript : Enemy
{
    [SerializeField] private GameObject StingerPrefab;
    [SerializeField] private GameObject MeleePrefab;

    private const float ATTACK_CD = 0.8f;
    private const float SHOOT_CD = 0.6f;

    private Vector2 lastMoveDirection;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Patrol, AIMode.Patrol, 6.0f);

        floating = true;
        lastMoveDirection = new Vector2(0, -1);
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // stinger projectile
            cooldowns[0] = SHOOT_CD;
            Projectile stinger = CreateAbility(StingerPrefab).GetComponent<Projectile>();
            stinger.SetDirection(lastMoveDirection);
        }
        else if(UseAbility(1)) {
            // contact damage attack
            cooldowns[1] = ATTACK_CD;
            CreateAbility(MeleePrefab).transform.parent = transform;
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
        if(controller.Target == null) {
            return;
        }

        if(cooldowns[1] <= 0 && controller.GetTargetDistance() <= 1.0f) {
            controller.QueueAbility(1);
        }
        else if(cooldowns[0] <= 0) {
            controller.QueueAbility(0);
        }
    }
}

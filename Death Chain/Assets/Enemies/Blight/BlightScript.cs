using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightScript : Enemy
{
    [SerializeField] private Sprite[] attackSprites;
    [SerializeField] private GameObject BlastPrefab;
    [SerializeField] private float blastCooldown;

    private Animation attackAnimation;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, 0.0f);

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.4f);
        walkAnimation = new Animation(walkSprites, AnimationType.Oscillate, 0.4f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        attackAnimation = new Animation(attackSprites, AnimationType.Rebound, 0.1f);

        floating = true;
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // use blast ability
            cooldowns[0] = blastCooldown;

            currentAnimation = attackAnimation;
            attackAnimation.Reset();

            CreateAttack(BlastPrefab);
        }
    }

    public override void AIUpdate(AIController controller) {
        if(cooldowns[0] <= 0 && controller.GetMoveDirection() == Vector2.zero) {
            controller.QueueAbility(0, 0.8f, 0.2f);
        }
    }

    protected override void OnDeath() {
        // poison blast on death
        CreateAttack(BlastPrefab);
    }
}

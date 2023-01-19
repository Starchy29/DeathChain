using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightScript : Enemy
{
    [SerializeField] private Sprite[] attackSprites;
    [SerializeField] private GameObject BlastPrefab;
    private const float BLAST_CD = 1.2f;
    private float blastCooldown;

    private Animation attackAnimation;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, 0.0f);
        maxSpeed = 7.0f;

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.4f);
        walkAnimation = new Animation(walkSprites, AnimationType.Oscillate, 0.4f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        attackAnimation = new Animation(attackSprites, AnimationType.Rebound, 0.1f);
    }

    protected override void UpdateAbilities() {
        if(blastCooldown > 0) {
            blastCooldown -= Time.deltaTime;
        } 
        else if(controller.GetUsedAbility() == 0) {
            // use blast ability
            blastCooldown = BLAST_CD;

            currentAnimation = attackAnimation;
            attackAnimation.Reset();

            GameObject blast = Instantiate(BlastPrefab);
            blast.transform.position = transform.position;
            blast.GetComponent<Attack>().User = gameObject;
        }
    }

    public override void AIUpdate(AIController controller) {
        if(blastCooldown <= 0 && controller.GetMoveDirection() == Vector2.zero) {
            controller.QueueAbility(0, 0.8f);
        }
    }
}

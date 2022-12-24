using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightScript : Enemy
{
    [SerializeField] private GameObject BlastPrefab;
    private const float BLAST_CD = 1.0f;
    private const float BLAST_RANGE = 2.0f; // how far away an AI will choose to explode
    private float blastCooldown;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, BLAST_RANGE + 1.0f);
        isAlly = false;
        maxSpeed = 7.0f;
    }

    protected override void UpdateAbilities() {
        if(blastCooldown > 0) {
            blastCooldown -= Time.deltaTime;
        } 
        else if(controller.GetUsedAbility() == 0) {
            // use blast ability
            blastCooldown = BLAST_CD;
            GameObject blast = Instantiate(BlastPrefab);
            blast.transform.position = transform.position;
            blast.GetComponent<Attack>().User = gameObject;
        }
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target != null && blastCooldown <= 0 &&!controller.AbilityQueued && controller.GetTargetDistance() <= BLAST_RANGE) {
            controller.QueueAbility(0, 0.5f);
        }
    }
}

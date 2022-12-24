using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightScript : Enemy
{
    [SerializeField] private GameObject BlastPrefab;
    private const float BLAST_CD = 1.0f;
    private float blastCooldown;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, AIMode.Wander);
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
}

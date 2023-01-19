using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomScript : Enemy
{
    [SerializeField] private GameObject sporePrefab;

    private const float SHOOT_CD = 0.8f;
    private const float WARP_CD = 3.0f;
    private float shootCooldown;
    private float warpCooldown;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Still, 6.0f);
        maxSpeed = 0.0f;
        sturdy = true;
    }

    protected override void UpdateAbilities() {
        int ability = controller.GetUsedAbility();

        if(shootCooldown > 0) {
            shootCooldown -= Time.deltaTime;
        }
        else if(ability == 0) {
            shootCooldown = SHOOT_CD;
            GameObject shot = Instantiate(sporePrefab);
            shot.transform.position = transform.position;
            Projectile script = shot.GetComponent<Projectile>();
            script.User = this.gameObject;
            script.SetDirection(controller.GetAimDirection());
        }

        //if(warpCooldown > 0) {
        //    warpCooldown -= Time.deltaTime;
        //}
        //else if(ability == 1) {

        //}
    }

    public override void AIUpdate(AIController controller) {
        if(shootCooldown <= 0 && controller.Target != null) {
            controller.QueueAbility(0, 0.3f);
        }
    }
}

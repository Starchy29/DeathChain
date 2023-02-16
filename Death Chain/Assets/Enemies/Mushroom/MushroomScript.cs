using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomScript : Enemy
{
    [SerializeField] private GameObject sporePrefab;
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float warpCooldown;
    
    private GameObject selector;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Still, 6.0f);
        maxSpeed = 0.0f;
        sturdy = true;

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.4f);
    }

    protected override void UpdateAbilities() {
        if(selector != null) {
            Vector3 selectPos = transform.position;
            Vector2 aim = controller.GetAimDirection();
            if(aim != Vector2.zero) {
                // find corpse closest to aim direction
                float bestDot = -1;
                List<GameObject> enemies = GameObject.Find("EntityTracker").GetComponent<EntityTracker>().Enemies;
                foreach(GameObject enemy in enemies) {
                    if(enemy.GetComponent<Enemy>().IsCorpse) {
                        float dot = Vector2.Dot(aim, (enemy.transform.position - transform.position).normalized);
                        if(dot > bestDot) {
                            bestDot = dot;
                            selectPos = enemy.transform.position;
                        }
                    }
                }
            }

            selector.transform.position = selectPos;

            // teleport on release
            if(controller.GetReleasedAbility() == 1) {
                cooldowns[1] = warpCooldown;
                if(selectPos == transform.position) {
                    cooldowns[1] = 0.5f; // shorter cooldown if no actual teleport
                }

                transform.position = selectPos;
                Destroy(selector);
                selector = null;
            }
            return;
        }

        int ability = controller.GetUsedAbility();

        if(cooldowns[0] <= 0 && ability == 0) {
            cooldowns[0] = shootCooldown;
            GameObject shot = Instantiate(sporePrefab);
            shot.transform.position = transform.position;
            Projectile script = shot.GetComponent<Projectile>();
            script.User = this.gameObject;
            script.SetDirection(controller.GetAimDirection());
        }
        else if(cooldowns[1] <= 0 && ability == 1) {
            selector = Instantiate(selectorPrefab);
            selector.transform.position = transform.position;
        }
    }

    public override void AIUpdate(AIController controller) {
        controller.ReleaseAbility = 1; // always instantly use teleport

        if(cooldowns[1] <= 0 && !controller.AbilityQueued && controller.Target != null && controller.GetTargetDistance() <= 2.0f) {
            // check for a potential warp target
            List<GameObject> enemies = GameObject.Find("EntityTracker").GetComponent<EntityTracker>().Enemies;
            foreach(GameObject enemy in enemies) {
                if(enemy.GetComponent<Enemy>().IsCorpse) {
                    controller.SetAim(controller.Target.gameObject.transform.position - transform.position); // try to move past attacker
                    controller.QueueAbility(1, 0);
                    break;
                }
            }
        }
        if(cooldowns[0] <= 0 && !controller.AbilityQueued && controller.Target != null) {
            controller.QueueAbility(0, 0.3f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomScript : Enemy
{
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] teleportSprites;
    [SerializeField] private GameObject sporePrefab;
    [SerializeField] private GameObject selectorPrefab;
    private const float SHOOT_CD = 1.2f;
    private const float WARP_CD = 3.0f;

    private Animation shootAnimation;
    private Animation teleportAnimation;
    private GameObject selector;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Still, AIMode.Still, 7.0f);
        ((AIController)controller).ReleaseAbility = 1; // ai always instantly uses teleport
        ((AIController)controller).IgnoreStart = true; // don't allow teleporting to affect vision
        sturdy = true;

        idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.4f);
        shootAnimation = new Animation(shootSprites, AnimationType.Rebound, 0.2f);
        deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        teleportAnimation = new Animation(teleportSprites, AnimationType.Forward, 0.3f);
    }

    protected override void UpdateAbilities() {
        if(selector != null) {
            if(currentAnimation == teleportAnimation) {
                return; // no actions while teleporting
            }

            Vector3 selectPos = transform.position;
            Vector2 aim = controller.GetAimDirection();
            if(aim != Vector2.zero) {
                // find corpse closest to aim direction
                float bestDot = -1;
                List<GameObject> enemies = EntityTracker.Instance.Enemies;
                foreach(GameObject enemy in enemies) {
                    if(enemy.GetComponent<Enemy>().IsCorpse && Vector2.Distance(enemy.transform.position, transform.position) < 12) {
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
                cooldowns[1] = WARP_CD;
                if(selectPos == transform.position) {
                    cooldowns[1] = 0.5f; // shorter cooldown if no actual teleport
                    Destroy(selector);
                    selector = null;
                } else {
                    teleportAnimation.ChangeType(AnimationType.Forward);
                    teleportAnimation.OnComplete = Teleport;
                    currentAnimation = teleportAnimation;
                    currentAnimation.Reset();
                }
            }
            return;
        }

        if(UseAbility(0)) {
            // fire spore
            cooldowns[0] = SHOOT_CD;
            CreateAttack(sporePrefab);
            currentAnimation = shootAnimation;
            shootAnimation.Reset();
        }
        else if(UseAbility(1)) {
            // teleport
            selector = Instantiate(selectorPrefab);
            selector.transform.position = transform.position;
        }
    }

    protected override void ResetAndClear() {
        if(selector != null) {
            Destroy(selector);
        }
    }

    private void Teleport() {
        transform.position = selector.transform.position;
        Destroy(selector);
        selector = null;

        currentAnimation.ChangeType(AnimationType.Reverse);
        currentAnimation.OnComplete = null;
        currentAnimation.Reset();
    }

    public override void AIUpdate(AIController controller) {
        if(cooldowns[1] <= 0 && controller.Target != null && controller.GetTargetDistance() <= 2.0f) {
            // check for a potential warp target
            List<GameObject> enemies = EntityTracker.Instance.Enemies;
            foreach(GameObject enemy in enemies) {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if(enemy.GetComponent<Enemy>().IsCorpse && distance > 2.0f && distance < 8.0f) {
                    controller.SetAim(controller.Target.gameObject.transform.position - transform.position); // try to move past attacker
                    controller.QueueAbility(1, 0);
                    break;
                }
            }
        }
        else if(cooldowns[0] <= 0 && controller.Target != null && !controller.IsTargetBlocked(false) ) {
            controller.QueueAbility(0, 0.3f);
        }
    }
}

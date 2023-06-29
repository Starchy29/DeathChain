using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderScript : Enemy
{
    [SerializeField] private GameObject ShotPrefab;
    [SerializeField] private GameObject WebPrefab;

    private const float WEB_CD = 6.0f;
    private const float CHARGE_RATE = 12.0f;
    private const float MAX_CHARGE = 16.0f;
    private const float CHARGE_WALK_SPEED = 2.0f;

    private bool charging;
    private float charge; // speed of the projectile

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, AIMode.Still, 5.5f);

        //idleAnimation = new Animation(idleSprites, AnimationType.Oscillate, 0.5f);
        //walkAnimation = new Animation(walkSprites, AnimationType.Loop, 0.5f);
        //deathAnimation = new Animation(deathSprites, AnimationType.Forward, 0.6f);
        //shootAnimation = new Animation(shootSprites, AnimationType.Rebound, 0.2f);
    }

    protected override void UpdateAbilities() {
        if(charging) {
            if(controller.GetReleasedAbility() == 0) {
                cooldowns[0] = 1.0f;
                GameObject shot = CreateAttack(ShotPrefab);
                shot.GetComponent<Projectile>().SetSpeed(charge);
                shot.GetComponent<Projectile>().ModifyDamage(charge / MAX_CHARGE);
                charging = false;
                ResetSpeed();
            } else {
                charge += CHARGE_RATE * Time.deltaTime * (statuses.HasStatus(Status.Energy) ? 1.5f : 1f);
                if(charge > MAX_CHARGE) {
                    charge = MAX_CHARGE;
                }
            }
            return;
        }

        if(UseAbility(0)) {
            // start pulling back "bow"
            charging = true;
            charge = 4.0f;
            SetNewSpeed(CHARGE_WALK_SPEED);
        }
        else if(UseAbility(1)) {
            // use lob web zone
            cooldowns[1] = WEB_CD;
            //currentAnimation = shootAnimation;
            //shootAnimation.Reset();
            CreateAttack(WebPrefab);
        }
    }

    protected override void ResetAndClear()
    {
        charging = false;
    }

    public override void AIUpdate(AIController controller) {
        if(controller.Target == null || charge >= Mathf.Min(MAX_CHARGE * controller.GetTargetDistance() / 5.0f, MAX_CHARGE) && !controller.IsTargetBlocked(false)) {
            // draw bow an amount proportional to how far the target is, but also release when the target is lost
            controller.ReleaseAbility = 0;
        } else {
            controller.ReleaseAbility = -1;
        }

        if(charging) {
            return;
        }

        if(cooldowns[1] <= 0) {
            // use web zone whenever available, random aim when no target is near
            if(controller.Target == null) {
                controller.SetAim(new Vector2(Random.value - 0.5f, Random.value - 0.5f));
            }
            controller.QueueAbility(1, 0.3f, 0.3f);
        }
        else if(cooldowns[0] <= 0 && controller.Target != null && controller.GetMoveDirection() == Vector2.zero) {
            // draw bow when not moving
            controller.QueueAbility(0);
        }
    }
}
